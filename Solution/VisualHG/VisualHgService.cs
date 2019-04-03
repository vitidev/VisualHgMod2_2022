using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EnvDTE;
using HgLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VisualHg.Images;

namespace VisualHg
{
    [Guid(Guids.Service)]
    public sealed class VisualHgService : IDisposable,
        IVsSccProvider, IVsSccGlyphs, IVsSccManager2, IVsSccManagerTooltip,
        IVsSolutionEvents, IVsUpdateSolutionEvents, IVsQueryEditQuerySave2, IVsTrackProjectDocumentsEvents2
    {
        private const int UpdateInterval = 100;

        private static bool StatusIconsLimited => VisualHgPackage.VsVersion < 11;

        private uint iconBaseIndex;
        private ImageList statusImageList;

        private bool _active;

        private uint vsSolutionEventsCookie = VSConstants.VSCOOKIE_NIL;
        private uint trackProjectDocumentsEventsCookie = VSConstants.VSCOOKIE_NIL;
        private readonly uint buildManagerCookie = VSConstants.VSCOOKIE_NIL;

        private DateTime lastUpdate;

        private readonly VisualHgRepository repository;
        private readonly IdleWorker worker;


        public bool Active
        {
            get => _active;
            set
            {
                if (value && !_active)
                {
                    var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;

                    repository.UpdateSolution(solution);
                }

                _active = value;
            }
        }

        public HgFileInfo[] PendingFiles => repository.PendingFiles;


        public VisualHgService()
        {
            worker = new IdleWorker();
            worker.DoWork += (s, e) => Update();

            repository = new VisualHgRepository();
            repository.StatusChanged += OnRepositoryStatusChanged;
            repository.SolutionFiles.Changed += (s, e) => UpdatePendingChangesToolWindow();

            var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            solution.AdviseSolutionEvents(this, out vsSolutionEventsCookie);
            Debug.Assert(vsSolutionEventsCookie != VSConstants.VSCOOKIE_NIL);

            var trackProjectDocuments = Package.GetGlobalService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
            trackProjectDocuments.AdviseTrackProjectDocumentsEvents(this, out trackProjectDocumentsEventsCookie);
            Debug.Assert(trackProjectDocumentsEventsCookie != VSConstants.VSCOOKIE_NIL);

            var buildManager = Package.GetGlobalService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
            buildManager.AdviseUpdateSolutionEvents(this, out buildManagerCookie);
            Debug.Assert(buildManagerCookie != VSConstants.VSCOOKIE_NIL);
        }

        public void Dispose()
        {
            worker.Dispose();

            repository.StatusChanged -= OnRepositoryStatusChanged;
            repository.Dispose();

            statusImageList.Dispose();

            if (vsSolutionEventsCookie != VSConstants.VSCOOKIE_NIL)
            {
                var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
                solution.UnadviseSolutionEvents(vsSolutionEventsCookie);
                vsSolutionEventsCookie = VSConstants.VSCOOKIE_NIL;
            }

            if (trackProjectDocumentsEventsCookie != VSConstants.VSCOOKIE_NIL)
            {
                var trackProjectDocuments = Package.GetGlobalService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
                trackProjectDocuments.UnadviseTrackProjectDocumentsEvents(trackProjectDocumentsEventsCookie);
                trackProjectDocumentsEventsCookie = VSConstants.VSCOOKIE_NIL;
            }

            if (buildManagerCookie != VSConstants.VSCOOKIE_NIL)
            {
                var buildManager = Package.GetGlobalService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
                buildManager.UnadviseUpdateSolutionEvents(buildManagerCookie);
            }
        }


        public HgFileStatus GetFileStatus(string filename)
        {
            return repository.GetFileStatus(filename);
        }


        private void OnRepositoryStatusChanged(object sender, EventArgs e)
        {
            if (EnoughTimePassedSinceLastUpdate())
            {
                worker.RequestDoWork();
            }
        }

        private bool EnoughTimePassedSinceLastUpdate()
        {
            return (DateTime.Now - lastUpdate).Milliseconds > UpdateInterval;
        }

        private void Update()
        {
            lastUpdate = DateTime.Now;

            UpdateStatusIcons();
            UpdatePendingChangesToolWindow();
            UpdateMainWindowCaption();
        }

        private void UpdateStatusIcons()
        {
            UpdateSolutionStatusIcon();
            UpdateLoadedProjectsStatusIcons();
        }

        private void UpdateSolutionStatusIcon()
        {
            var hierarchy = Package.GetGlobalService(typeof(SVsSolution)) as IVsHierarchy;
            var property = (int)__VSHPROPID.VSHPROPID_StateIconIndex;
            var icon = GetStatusIcon(VisualHgSolution.SolutionFileName);

            hierarchy.SetProperty(VSConstants.VSITEMID_ROOT, property, icon);
        }

        private void UpdateLoadedProjectsStatusIcons()
        {
            foreach (var project in VisualHgSolution.LoadedProjects)
            {
                UpdateProjectStatusIcons(project);
            }
        }

        private void UpdateProjectStatusIcons(IVsHierarchy hierarchy)
        {
            var project = hierarchy as IVsSccProject2;
            if (project == null)
                return;

            project.SccGlyphChanged(0, null, null, null);

            if (VisualHgOptions.Global.ProjectStatusIncludesChildren)
            {
                var projectStatus = HgFileStatus.None;
                var projectUnderControl = false;

                foreach (var file in VisualHgSolution.GetProjectFiles(project).Where(f => f[f.Length - 1] != '\\'))
                {
                    if (projectStatus == HgFileStatus.Modified)
                        break;

                    var fileStatus = repository.GetFileStatus(file);
                    if (!projectUnderControl && fileStatus != HgFileStatus.NotTracked)
                        projectUnderControl = true;

                    if (fileStatus == HgFileStatus.Added)
                    {
                        projectStatus = HgFileStatus.Added;
                        continue;
                    }

                    if (fileStatus == HgFileStatus.Renamed || (fileStatus == HgFileStatus.Copied || fileStatus == HgFileStatus.Modified))
                    {
                        projectStatus = HgFileStatus.Modified;
                        continue;
                    }

                    if (fileStatus == HgFileStatus.None)
                    {
                        if (fileStatus == HgFileStatus.Clean)
                            projectStatus = HgFileStatus.Clean;
                    }
                }

                if (projectStatus == HgFileStatus.None && projectUnderControl)
                    projectStatus = HgFileStatus.Clean;

                var rgsiGlyphs = new[] { GetStatusIcon(projectStatus) };
                var rgdwSccStatus = new[] { (uint)projectStatus };
                var rguiAffectedNodes = new[] { VSConstants.VSITEMID_ROOT };
                project.SccGlyphChanged(1, rguiAffectedNodes, rgsiGlyphs, rgdwSccStatus);
            }
        }


        private void UpdatePendingChangesToolWindow()
        {
            var visualHg = Package.GetGlobalService(typeof(IServiceProvider)) as VisualHgPackage;

            visualHg.UpdatePendingChangesToolWindow();
        }

        private void UpdateMainWindowCaption()
        {
            var branches = repository.Branches;
            var text = branches.Length > 0 ? branches.Distinct().Aggregate((x, y) => String.Concat(x, ", ", y)) : "";

            UpdateMainWindowCaption(text);
        }

        private static void UpdateMainWindowCaption(string text)
        {
            var dte = Package.GetGlobalService(typeof(SDTE)) as _DTE;

            if (dte == null || dte.MainWindow == null)
            {
                return;
            }

            var caption = dte.MainWindow.Caption;
            var additionalInfo = String.IsNullOrEmpty(text) ? "" : String.Concat(" (", text, ") ");

            var newCaption = Regex.Replace(caption,
                @"^(?<Solution>[^\(]+)(?<AdditionalInfo> \(.+\))? (?<Application>- [^\(]+) (?<User>\(.+\)) ?(?<Instance>- .+)?$",
                String.Concat("${Solution}", additionalInfo, "${Application} ${User} ${Instance}"));

            if (caption != newCaption)
            {
                SetWindowText((IntPtr)dte.MainWindow.HWnd, newCaption);
            }
        }

        private static void SetWindowText(IntPtr handle, string text)
        {
            try
            {
                NativeMethods.SetWindowText(handle, text);
            }
            catch { }
        }



        private bool AnyItemsUnderSourceControl => Active && !repository.IsEmpty;


        private void InitializeStatusImageList(uint baseIndex)
        {
            iconBaseIndex = baseIndex;

            if (statusImageList == null)
            {
                var fileName = VisualHgOptions.Global.StatusImageFileName;

                if (StatusIconsLimited)
                {
                    statusImageList = StatusImages.CreateLimited(fileName);
                }
                else
                {
                    statusImageList = StatusImages.Create(fileName);
                }
            }
        }

        private VsStateIcon GetStatusIcon(string fileName)
        {
            var status = repository.GetFileStatus(fileName);
            return GetStatusIcon(status);
        }

        private VsStateIcon GetStatusIcon(HgFileStatus status)
        {
            var iconIndex = 0;

            if (StatusIconsLimited)
            {
                iconIndex = StatusImages.GetIndexLimited(status);
            }
            else
            {
                iconIndex = StatusImages.GetIndex(status);
            }

            return GetStatusIcon(iconIndex);
        }

        private VsStateIcon GetStatusIcon(int iconIndex)
        {
            if (iconIndex == -1)
            {
                return VsStateIcon.STATEICON_BLANK;
            }

            return (VsStateIcon)(iconBaseIndex + iconIndex);
        }

        private void OnProjectRegister(IVsSccProject2 project)
        {
            if (project != null)
            {
                repository.UpdateProject(project);
            }
        }

        private string GetToolTipText(IVsHierarchy hierarchy, uint itemId)
        {
            var files = VisualHgSolution.GetItemFiles(hierarchy, itemId);

            if (files.Length == 0)
            {
                return "";
            }

            var fileName = files[0];

            var text = repository.GetFileStatus(fileName).ToString();
            var branch = repository.GetBranch(fileName);

            if (!String.IsNullOrEmpty(branch))
            {
                text += " (" + branch + ")";
            }

            return text;
        }


        private void OnAfterCloseSolution()
        {
            VisualHgSolution.LastSeenProjectDirectory = "";

            repository.Clear();
            UpdatePendingChangesToolWindow();
        }

        private void OnAfterLoadProject(IVsHierarchy hierarchy)
        {
            var project = hierarchy as IVsSccProject2;

            if (project != null)
            {
                repository.UpdateProject(project);
            }

            UpdateLastSeenProjectDirectory(hierarchy);
        }

        private void OnAfterOpenProject(IVsHierarchy hierarchy)
        {
            var files = VisualHgSolution.GetProjectFiles(hierarchy);

            foreach (var root in files.Select(x => HgPath.FindRepositoryRoot(x)).Distinct())
            {
                repository.UpdateRootStatus(root);
            }

            AddIf(VisualHgOptions.Global.AddFilesOnLoad, files);

            UpdateLastSeenProjectDirectory(hierarchy);
        }

        private static void UpdateLastSeenProjectDirectory(IVsHierarchy hierarchy)
        {
            VisualHgSolution.LastSeenProjectDirectory = VisualHgSolution.GetDirectoryName(hierarchy);
        }

        private void OnAfterOpenSolution()
        {
            if (!Active &&
                VisualHgOptions.Global.AutoActivatePlugin &&
                VisualHgSolution.IsUnderSourceControl)
            {
                VisualHgPackage.RegisterSourceControlProvider();
            }

            repository.SolutionFiles.Add(VisualHgSolution.SolutionFileName);
        }

        private void OnBeforeCloseOrUnloadProject(IVsHierarchy hierarchy)
        {
            var files = VisualHgSolution.GetProjectFiles(hierarchy);

            repository.SolutionFiles.Remove(files);
        }


        private void OnSolutionBuildEnded()
        {
            repository.SolutionBuildEnded();
        }

        private void OnSolutionBuildStarted()
        {
            repository.SolutionBuildStarted();
        }

        private void OnFileSave(params string[] fileNames)
        {
            if (Active)
            {
                repository.UpdateFileStatus(fileNames);
            }
        }


        private void OnAfterAddFiles(string[] fileNames)
        {
            AddIf(VisualHgOptions.Global.AutoAddNewFiles, fileNames);
        }

        private void OnAfterRemoveFiles(string[] fileNames)
        {
            var filesDeletedFromDisk = fileNames.Where(x => !File.Exists(x)).ToArray();

            if (filesDeletedFromDisk.Length > 0)
            {
                repository.RemoveFiles(filesDeletedFromDisk);
            }

            repository.SolutionFiles.Remove(fileNames);
        }

        private void OnAfterRenameFiles(string[] fileNames, string[] newFileNames)
        {
            repository.RenameFiles(fileNames, newFileNames);
        }



        private void AddIf(bool condition, string[] files)
        {
            if (condition)
            {
                repository.AddFiles(files);
            }
            else
            {
                repository.SolutionFiles.Add(files);
            }
        }


        #region Interfaces implementation

        int IVsSccProvider.AnyItemsUnderSourceControl(out int pfResult)
        {
            pfResult = AnyItemsUnderSourceControl ? 1 : 0;
            return VSConstants.S_OK;
        }

        int IVsSccProvider.SetActive()
        {
            Active = true;
            return VSConstants.S_OK;
        }

        int IVsSccProvider.SetInactive()
        {
            Active = false;
            return VSConstants.S_OK;
        }


        int IVsSccGlyphs.GetCustomGlyphList(uint BaseIndex, out uint pdwImageListHandle)
        {
            InitializeStatusImageList(BaseIndex);

            pdwImageListHandle = unchecked((uint)statusImageList.Handle);

            return VSConstants.S_OK;
        }


        int IVsSccManager2.BrowseForProject(out string pbstrDirectory, out int pfOK)
        {
            pbstrDirectory = null;
            pfOK = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IVsSccManager2.CancelAfterBrowseForProject()
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsSccManager2.GetSccGlyph(int cFiles, string[] rgpszFullPaths, VsStateIcon[] rgsiGlyphs, uint[] rgdwSccStatus)
        {
            if (cFiles == 0 || String.IsNullOrEmpty(rgpszFullPaths[0]))
            {
                return VSConstants.S_OK;
            }

            if (rgdwSccStatus.Length == 1)
            {
                rgsiGlyphs[0] = GetStatusIcon(rgpszFullPaths[0]);
            }
            else
            {
                var entries = new List<FileOrDirEntry>();
                for (int i = 0; i < rgpszFullPaths.Length; i++)
                {
                    var fullPath = rgpszFullPaths[i];
                    var status = repository.GetFileStatus(fullPath);
                    var entry = new FileOrDirEntry(fullPath, status, i);

                    foreach (var dirEntry in entries.Where(e => e.IsDirectory))
                    {
                        if (dirEntry.Contains(entry))
                            dirEntry.UpdateStatus(entry.Status);
                    }

                    entries.Add(entry);
                }

                foreach (var entry in entries)
                {
                    rgdwSccStatus[entry.Index] = (uint)entry.Status;
                    rgsiGlyphs[entry.Index] = GetStatusIcon(entry.Status);
                }
            }

            return VSConstants.S_OK;
        }

        int IVsSccManager2.GetSccGlyphFromStatus(uint dwSccStatus, VsStateIcon[] psiGlyph)
        {
            return VSConstants.S_OK;
        }

        int IVsSccManager2.IsInstalled(out int pbInstalled)
        {
            pbInstalled = 1;
            return VSConstants.S_OK;
        }

        int IVsSccManager2.RegisterSccProject(IVsSccProject2 pscp2Project, string pszSccProjectName, string pszSccAuxPath, string pszSccLocalPath, string pszProvider)
        {
            OnProjectRegister(pscp2Project);
            return VSConstants.S_OK;
        }

        int IVsSccManager2.UnregisterSccProject(IVsSccProject2 pscp2Project)
        {
            return VSConstants.S_OK;
        }


        int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Cancel()
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            OnSolutionBuildEnded();
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            OnSolutionBuildStarted();
            return VSConstants.S_OK;
        }


        int IVsSccManagerTooltip.GetGlyphTipText(IVsHierarchy phierHierarchy, uint itemidNode, out string pbstrTooltipText)
        {
            pbstrTooltipText = GetToolTipText(phierHierarchy, itemidNode);
            return VSConstants.S_OK;
        }


        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            OnAfterCloseSolution();
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            OnAfterLoadProject(pRealHierarchy);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            OnAfterOpenProject(pHierarchy);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            OnAfterOpenSolution();
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            OnBeforeCloseOrUnloadProject(pHierarchy);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            OnBeforeCloseOrUnloadProject(pRealHierarchy);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }


        int IVsQueryEditQuerySave2.BeginQuerySaveBatch()
        {
            return VSConstants.S_OK;
        }

        int IVsQueryEditQuerySave2.DeclareReloadableFile(string pszMkDocument, uint rgf, VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        int IVsQueryEditQuerySave2.DeclareUnreloadableFile(string pszMkDocument, uint rgf, VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        int IVsQueryEditQuerySave2.EndQuerySaveBatch()
        {
            return VSConstants.S_OK;
        }

        int IVsQueryEditQuerySave2.IsReloadable(string pszMkDocument, out int pbResult)
        {
            // Since we're not tracking which files are reloadable and which not, consider everything reloadable
            pbResult = 1;
            return VSConstants.S_OK;
        }

        int IVsQueryEditQuerySave2.OnAfterSaveUnreloadableFile(string pszMkDocument, uint rgf, VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        int IVsQueryEditQuerySave2.QueryEditFiles(uint rgfQueryEdit, int cFiles, string[] rgpszMkDocuments, uint[] rgrgf, VSQEQS_FILE_ATTRIBUTE_DATA[] rgFileInfo, out uint pfEditVerdict, out uint prgfMoreInfo)
        {
            pfEditVerdict = (uint)tagVSQueryEditResult.QER_EditOK;
            prgfMoreInfo = 0;
            return VSConstants.S_OK;
        }

        int IVsQueryEditQuerySave2.QuerySaveFile(string pszMkDocument, uint rgf, VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo, out uint pdwQSResult)
        {
            OnFileSave(pszMkDocument);
            pdwQSResult = (uint)tagVSQuerySaveResult.QSR_SaveOK;
            return VSConstants.S_OK;
        }

        int IVsQueryEditQuerySave2.QuerySaveFiles(uint rgfQuerySave, int cFiles, string[] rgpszMkDocuments, uint[] rgrgf, VSQEQS_FILE_ATTRIBUTE_DATA[] rgFileInfo, out uint pdwQSResult)
        {
            OnFileSave(rgpszMkDocuments);
            pdwQSResult = (uint)tagVSQuerySaveResult.QSR_SaveOK;
            return VSConstants.S_OK;
        }


        int IVsTrackProjectDocumentsEvents2.OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            OnAfterAddFiles(rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            if (rgpProjects == null || rgpszMkDocuments == null)
            {
                return VSConstants.E_POINTER;
            }

            OnAfterRemoveFiles(rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            OnAfterRenameFiles(rgszMkOldNames, rgszMkNewNames);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags, VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        #endregion

        private class FileOrDirEntry
        {
            public string Path { get; }
            public HgFileStatus Status { get; private set; }
            public int Index { get; }
            public bool IsDirectory { get; }

            public FileOrDirEntry(string path, HgFileStatus status, int index)
            {
                Path = path;
                Status = status;
                Index = index;
                IsDirectory = path[path.Length - 1] == '\\';
            }

            public void UpdateStatus(HgFileStatus status)
            {
                if (Status == HgFileStatus.Modified)
                    return;

                if (status == HgFileStatus.Added)
                {
                    Status = HgFileStatus.Added;
                    return;
                }

                if (status == HgFileStatus.Renamed || status == HgFileStatus.Copied || status == HgFileStatus.Modified)
                {
                    Status = HgFileStatus.Modified;
                    return;
                }

                if (Status == HgFileStatus.NotTracked)
                {
                    if (status == HgFileStatus.Clean)
                        Status = HgFileStatus.Clean;
                }
            }

            public bool Contains(FileOrDirEntry fileOrDirEntry) => fileOrDirEntry.Path.StartsWith(Path);
        }
    }
}