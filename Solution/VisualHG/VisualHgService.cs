using EnvDTE;
using HgLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VisualHg.Images;

namespace VisualHg
{
    [Guid(Guids.Service)]
    public sealed class VisualHgService : IDisposable,
        IVsSccProvider, IVsSccGlyphs2, IVsSccManager2, IVsSccManagerTooltip,
        IVsSolutionEvents, IVsUpdateSolutionEvents, IVsQueryEditQuerySave2, IVsTrackProjectDocumentsEvents2
    {
        private const int UpdateInterval = 100;

        private static bool StatusIconsLimited => VisualHgPackage.VsVersion < 11;

        private uint iconBaseIndex = (uint)VsStateIcon.STATEICON_MAXINDEX;
        private ImageList statusImageList;

        private bool _active;

        // ReSharper disable MemberInitializerValueIgnored
        private uint vsSolutionEventsCookie = VSConstants.VSCOOKIE_NIL;
        private uint trackProjectDocumentsEventsCookie = VSConstants.VSCOOKIE_NIL;
        private readonly uint buildManagerCookie = VSConstants.VSCOOKIE_NIL;
        // ReSharper restore MemberInitializerValueIgnored

        private DateTime lastUpdate;

        private readonly VisualHgRepository repository;
        private readonly IdleWorker worker;


        public bool Active
        {
            get => _active;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (value && !_active)
                {
                    var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));

                    repository.UpdateSolution(solution);
                }

                _active = value;
            }
        }

        public HgFileInfo[] PendingFiles => repository.PendingFiles;


        public VisualHgService()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            worker = new IdleWorker();
            worker.DoWork += (s, e) => Update();

            repository = new VisualHgRepository();
            repository.StatusChanged += OnRepositoryStatusChanged;
            repository.SolutionFiles.Changed += (s, e) => UpdatePendingChangesToolWindow();

            var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            solution.AdviseSolutionEvents(this, out vsSolutionEventsCookie);
            Debug.Assert(vsSolutionEventsCookie != VSConstants.VSCOOKIE_NIL);

            var trackProjectDocuments =
                (IVsTrackProjectDocuments2)Package.GetGlobalService(typeof(SVsTrackProjectDocuments));
            trackProjectDocuments.AdviseTrackProjectDocumentsEvents(this, out trackProjectDocumentsEventsCookie);
            Debug.Assert(trackProjectDocumentsEventsCookie != VSConstants.VSCOOKIE_NIL);

            var buildManager = (IVsSolutionBuildManager)Package.GetGlobalService(typeof(SVsSolutionBuildManager));
            buildManager.AdviseUpdateSolutionEvents(this, out buildManagerCookie);
            Debug.Assert(buildManagerCookie != VSConstants.VSCOOKIE_NIL);
        }

        public void Dispose()
        {
            worker.Dispose();

            repository.StatusChanged -= OnRepositoryStatusChanged;
            repository.Dispose();

            statusImageList?.Dispose();

            ThreadHelper.ThrowIfNotOnUIThread();

            if (vsSolutionEventsCookie != VSConstants.VSCOOKIE_NIL)
            {
                var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
                solution.UnadviseSolutionEvents(vsSolutionEventsCookie);
                vsSolutionEventsCookie = VSConstants.VSCOOKIE_NIL;
            }

            if (trackProjectDocumentsEventsCookie != VSConstants.VSCOOKIE_NIL)
            {
                var trackProjectDocuments =
                    (IVsTrackProjectDocuments2)Package.GetGlobalService(typeof(SVsTrackProjectDocuments));
                trackProjectDocuments.UnadviseTrackProjectDocumentsEvents(trackProjectDocumentsEventsCookie);
                trackProjectDocumentsEventsCookie = VSConstants.VSCOOKIE_NIL;
            }

            if (buildManagerCookie != VSConstants.VSCOOKIE_NIL)
            {
                var buildManager = (IVsSolutionBuildManager)Package.GetGlobalService(typeof(SVsSolutionBuildManager));
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
                worker.RequestDoWork();
        }

        private bool EnoughTimePassedSinceLastUpdate()
        {
            return (DateTime.Now - lastUpdate).Milliseconds > UpdateInterval;
        }

        private void Update()
        {
            lastUpdate = DateTime.Now;

            ThreadHelper.ThrowIfNotOnUIThread();

            UpdateStatusIcons();
            UpdatePendingChangesToolWindow();
            UpdateMainWindowCaption();
        }

        private void UpdateStatusIcons()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            UpdateSolutionStatusIcon();
            UpdateLoadedProjectsStatusIcons();
        }

        private void UpdateSolutionStatusIcon()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var hierarchy = (IVsHierarchy)Package.GetGlobalService(typeof(SVsSolution));
            var property = (int)__VSHPROPID.VSHPROPID_StateIconIndex;
            var icon = GetStatusIcon(VisualHgSolution.SolutionFileName);

            hierarchy.SetProperty(VSConstants.VSITEMID_ROOT, property, icon);
        }

        private void UpdateLoadedProjectsStatusIcons()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (var project in VisualHgSolution.LoadedProjects)
                UpdateProjectStatusIcons(project);
        }

        private void UpdateProjectStatusIcons(IVsHierarchy hierarchy)
        {
            if (!(hierarchy is IVsSccProject2 project))
                return;

            ThreadHelper.ThrowIfNotOnUIThread();

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

                    if (fileStatus == HgFileStatus.Renamed || fileStatus == HgFileStatus.Copied ||
                        fileStatus == HgFileStatus.Modified)
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
            var visualHg = (VisualHgPackage)Package.GetGlobalService(typeof(IServiceProvider));

            visualHg.UpdatePendingChangesToolWindow();
        }

        private void UpdateMainWindowCaption()
        {
            var branches = repository.Branches;
            var text = branches.Length > 0
                ? branches.Distinct().Aggregate((x, y) => string.Concat(x, ", ", y))
                : "";

            UpdateMainWindowCaption(text);
        }

        private static void UpdateMainWindowCaption(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = Package.GetGlobalService(typeof(SDTE)) as _DTE;

            if (dte?.MainWindow == null)
                return;

            var caption = dte.MainWindow.Caption;
            var additionalInfo = string.IsNullOrEmpty(text) ? "" : string.Concat(" (", text, ") ");

            var newCaption = Regex.Replace(caption,
                @"^(?<Solution>[^\(]+)(?<AdditionalInfo> \(.+\))? (?<Application>- [^\(]+) (?<User>\(.+\)) ?(?<Instance>- .+)?$",
                string.Concat("${Solution}", additionalInfo, "${Application} ${User} ${Instance}"));

            if (caption != newCaption)
                SetWindowText((IntPtr)dte.MainWindow.HWnd, newCaption);
        }

        private static void SetWindowText(IntPtr handle, string text)
        {
            try
            {
                NativeMethods.SetWindowText(handle, text);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
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
            var iconIndex = StatusIconsLimited ? StatusImages.GetIndexLimited(status) : StatusImages.GetIndex(status);

            return GetStatusIcon(iconIndex);
        }

        private VsStateIcon GetStatusIcon(int iconIndex)
        {
            if (iconIndex == -1)
                return VsStateIcon.STATEICON_BLANK;

            return (VsStateIcon)(iconBaseIndex + iconIndex);
        }

        private void OnProjectRegister(IVsSccProject2 project)
        {
            if (project != null)
                repository.UpdateProject(project);
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

            if (!string.IsNullOrEmpty(branch))
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
            ThreadHelper.ThrowIfNotOnUIThread();

            if (hierarchy is IVsSccProject2 project)
            {
                repository.UpdateProject(project);
            }

            UpdateLastSeenProjectDirectory(hierarchy);
        }

        private void OnAfterOpenProject(IVsHierarchy hierarchy)
        {
            var files = VisualHgSolution.GetProjectFiles(hierarchy);

            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (var root in files.Select(HgPath.FindRepositoryRoot).Distinct())
            {
                repository.UpdateRootStatus(root);
            }

            AddIf(VisualHgOptions.Global.AddFilesOnLoad, files);

            UpdateLastSeenProjectDirectory(hierarchy);
        }

        private static void UpdateLastSeenProjectDirectory(IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            VisualHgSolution.LastSeenProjectDirectory = VisualHgSolution.GetDirectoryName(hierarchy);
        }

        private void OnAfterOpenSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

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
            ThreadHelper.ThrowIfNotOnUIThread();

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
                repository.UpdateFileStatus(fileNames);
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
                repository.AddFiles(files);
            else
                repository.SolutionFiles.Add(files);
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

        int IVsSccManager2.GetSccGlyph(int cFiles, string[] rgpszFullPaths, VsStateIcon[] rgsiGlyphs,
            uint[] rgdwSccStatus)
        {
            if (cFiles == 0 || string.IsNullOrEmpty(rgpszFullPaths[0]))
            {
                return VSConstants.S_OK;
            }

            if (rgdwSccStatus != null)
            {
                if (rgdwSccStatus.Length == 1)
                {
                    rgsiGlyphs[0] = GetStatusIcon(rgpszFullPaths[0]);
                }
                else
                {
                    var entries = new List<FileOrDirEntry>();
                    for (var i = 0; i < rgpszFullPaths.Length; i++)
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

                    bool prjModified = false;
                    foreach (var entry in entries)
                    {
                        if (!prjModified && entry.Status != HgFileStatus.Clean 
                            && entry.Status != HgFileStatus.None 
                            && entry.Status != HgFileStatus.Ignored
                            && entry.Status != HgFileStatus.Missing
                            )
                            prjModified = true;
                        rgdwSccStatus[entry.Index] = (uint)entry.Status;
                        rgsiGlyphs[entry.Index] = GetStatusIcon(entry.Status);
                    }

                    if (prjModified)
                    {
                        var prjFileIndex = Array.FindIndex(rgpszFullPaths, i => i.EndsWith(".csproj"));
                        if (prjFileIndex != -1)
                        {
                            rgdwSccStatus[prjFileIndex] = (uint)HgFileStatus.Modified;
                            rgsiGlyphs[prjFileIndex] = GetStatusIcon(HgFileStatus.Modified);
                        }
                    }
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

        int IVsSccManager2.RegisterSccProject(IVsSccProject2 pscp2Project, string pszSccProjectName,
            string pszSccAuxPath, string pszSccLocalPath, string pszProvider)
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


        int IVsSccManagerTooltip.GetGlyphTipText(IVsHierarchy phierHierarchy, uint itemidNode,
            out string pbstrTooltipText)
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

        int IVsQueryEditQuerySave2.DeclareReloadableFile(string pszMkDocument, uint rgf,
            VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        int IVsQueryEditQuerySave2.DeclareUnreloadableFile(string pszMkDocument, uint rgf,
            VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
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

        int IVsQueryEditQuerySave2.OnAfterSaveUnreloadableFile(string pszMkDocument, uint rgf,
            VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        int IVsQueryEditQuerySave2.QueryEditFiles(uint rgfQueryEdit, int cFiles, string[] rgpszMkDocuments,
            uint[] rgrgf, VSQEQS_FILE_ATTRIBUTE_DATA[] rgFileInfo, out uint pfEditVerdict, out uint prgfMoreInfo)
        {
            pfEditVerdict = (uint)tagVSQueryEditResult.QER_EditOK;
            prgfMoreInfo = 0;
            return VSConstants.S_OK;
        }

        int IVsQueryEditQuerySave2.QuerySaveFile(string pszMkDocument, uint rgf, VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo,
            out uint pdwQSResult)
        {
            OnFileSave(pszMkDocument);
            pdwQSResult = (uint)tagVSQuerySaveResult.QSR_SaveOK;
            return VSConstants.S_OK;
        }

        int IVsQueryEditQuerySave2.QuerySaveFiles(uint rgfQuerySave, int cFiles, string[] rgpszMkDocuments,
            uint[] rgrgf, VSQEQS_FILE_ATTRIBUTE_DATA[] rgFileInfo, out uint pdwQSResult)
        {
            OnFileSave(rgpszMkDocuments);
            pdwQSResult = (uint)tagVSQuerySaveResult.QSR_SaveOK;
            return VSConstants.S_OK;
        }


        int IVsTrackProjectDocumentsEvents2.OnAfterAddDirectoriesEx(int cProjects, int cDirectories,
            IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects,
            int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            OnAfterAddFiles(rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveDirectories(int cProjects, int cDirectories,
            IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects,
            int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            if (rgpProjects == null || rgpszMkDocuments == null)
            {
                return VSConstants.E_POINTER;
            }

            OnAfterRemoveFiles(rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects,
            int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects,
            int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            OnAfterRenameFiles(rgszMkOldNames, rgszMkNewNames);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects,
            int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddDirectories(IVsProject pProject, int cDirectories,
            string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags, VSQUERYADDDIRECTORYRESULTS[] pSummaryResult,
            VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments,
            VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveDirectories(IVsProject pProject, int cDirectories,
            string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags,
            VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveFiles(IVsProject pProject, int cFiles,
            string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult,
            VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameDirectories(IVsProject pProject, int cDirs,
            string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags,
            VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames,
            string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult,
            VSQUERYRENAMEFILERESULTS[] rgResults)
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


        // Create a new moniker list to contain the new monikers.
        private readonly IVsImageMonikerImageList monikerList = new MonikerList();

        public IVsImageMonikerImageList GetCustomGlyphMonikerList(uint baseIndex)
        {
            return monikerList;
        }

        /// <summary>
        /// Define the custom monikers to be displayed in the moniker list. In this case, we are using 
        /// predefined image monikers from the known moniker list.
        /// </summary>
        private class MonikerList : IVsImageMonikerImageList
        {
            /// <summary>
            /// This list of custom monikers will be appended to the standard moniker list
            /// </summary>
            List<ImageMoniker> monikers = new List<ImageMoniker>
            {
                KnownMonikers.OnlineStatusBusy, //???
                KnownMonikers.OnlineStatusBusy, //Modified ++
                KnownMonikers.AddNoColor, //Added +++
                KnownMonikers.Cancel, //Removed
                KnownMonikers.OnlineStatusAvailable, //Clean
                KnownMonikers.OnlineStatusAway, //Missing
                KnownMonikers.Blank, //NotTracked
                KnownMonikers.OnlineStatusUnknown, //Ignored
                KnownMonikers.OnlineStatusOffline, //Renamed
                KnownMonikers.OnlineStatusOffline, //Copied
            };

            /// <summary>
            /// Function required by IVsImageMonikerList
            /// </summary>
            public int ImageCount
            {
                get
                {
                    return monikers.Count;
                }
            }

            /// <summary>
            /// Add custom image monikers to array of monikers.
            /// </summary>
            /// <param name="firstImageIndex">Index value of the first custom moniker to add.</param>
            /// <param name="imageMonikerCount">Number of image monikers to add to array</param>
            /// <param name="imageMonikers">Array of image monikers. Assign custom monikers to elements of this array</param>
            public void GetImageMonikers(int firstImageIndex, int imageMonikerCount, ImageMoniker[] imageMonikers)
            {
                for (int ii = 0; ii < imageMonikerCount; ii++)
                {
                    imageMonikers[ii] = monikers[firstImageIndex + ii];
                }
            }
        }
    }
}