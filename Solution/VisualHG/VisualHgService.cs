using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EnvDTE;
using HgLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    [Guid(Guids.Service)]
    public partial class VisualHgService : IDisposable,
        IVsSccProvider, IVsSccGlyphs, IVsSccManager2, IVsSccManagerTooltip,
        IVsSolutionEvents, IVsUpdateSolutionEvents, IVsQueryEditQuerySave2, IVsTrackProjectDocumentsEvents2
    {
        private uint iconBaseIndex;
        private ImageList statusImageList;
        
        private bool _active;
        private bool nodesGlyphsDirty = true;

        private uint vsSolutionEventsCookie = VSConstants.VSCOOKIE_NIL;
        private uint trackProjectDocumentsEventsCookie = VSConstants.VSCOOKIE_NIL;
        private uint buildManagerCookie = VSConstants.VSCOOKIE_NIL;

        private DateTime lastUpdate;

        private VisualHgRepository repository;
        private IdlenessNotifier idlenessNotifier;


        public bool Active
        {
            get { return _active; }
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

        public HgFileInfo[] PendingFiles
        {
            get { return repository.PendingFiles; }
        }


        public VisualHgService()
        {
            idlenessNotifier = new IdlenessNotifier();
            idlenessNotifier.Idle += UpdateDirtyNodesGlyphs;
            idlenessNotifier.Register();

            repository = new VisualHgRepository();
            repository.StatusChanged += SetNodesGlyphsDirty;

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
            idlenessNotifier.Idle -= UpdateDirtyNodesGlyphs;
            idlenessNotifier.Revoke();

            repository.StatusChanged -= SetNodesGlyphsDirty;
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


        private void SetNodesGlyphsDirty(object sender, EventArgs e)
        {
            nodesGlyphsDirty = true;
        }

        private void UpdateDirtyNodesGlyphs(object sender, EventArgs e)
        {
            if (nodesGlyphsDirty && (DateTime.Now - lastUpdate).Milliseconds > 100)
            {
                RefreshNodesGlyphs();
                UpdateMainWindowCaption();
                UpdatePendingChangesToolWindow();
            }
        }

        private void RefreshNodesGlyphs()
        {
            var nodes = new [] { GetSolutionVsItemSelection() }
                .Concat(VisualHgSolution.LoadedProjects.Select(GetVsItemSelection))
                .ToArray();

            VisualHgSolution.UpdateGlyphs(nodes);

            lastUpdate = DateTime.Now;
            nodesGlyphsDirty = false;
        }

        private static VSITEMSELECTION GetSolutionVsItemSelection()
        {
            var hierarchy = Package.GetGlobalService(typeof(SVsSolution)) as IVsHierarchy;

            return GetVsItemSelection(hierarchy);
        }

        private static VSITEMSELECTION GetVsItemSelection(IVsHierarchy hierarchy)
        {
            return new VSITEMSELECTION {
                itemid = VSConstants.VSITEMID_ROOT,
                pHier = hierarchy,
            };
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
                @"^(?<Solution>[^\(]+)(?<AdditionalInfo> \(.+\))? (?<Application>- [^\(]+) (?<User>\(.+\)) ?(?<Instance>- .+)$",
                String.Concat("${Solution}", additionalInfo, "${Application} ${User} ${Instance}"));

            if (caption != newCaption)
            {
                NativeMethods.SetWindowText((IntPtr)dte.MainWindow.HWnd, newCaption);
            }
        }



        private bool AnyItemsUnderSourceControl
        {
            get { return Active && !repository.IsEmpty; }
        }


        private void InitializeStatusImageList(uint baseIndex)
        {
            iconBaseIndex = baseIndex;

            if (statusImageList == null)
            {
                statusImageList = ImageMapper.CreateStatusImageList(Configuration.Global.StatusImageFileName);
            }
        }

        public VsStateIcon GetStateIcon(string fileName)
        {
            var status = repository.GetFileStatus(fileName);
            var iconIndex = ImageMapper.GetStatusIconIndex(status);

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
            var project = hierarchy as IVsSccProject2;
                        
            var files = repository.SolutionFiles.Add(hierarchy);

            foreach (var root in files.Select(x => HgPath.FindRepositoryRoot(x)).Distinct())
            {
                repository.UpdateRootStatus(root);
            }

            if (Configuration.Global.AddFilesOnLoad)
            {
                repository.AddFiles(files);
            }

            UpdateLastSeenProjectDirectory(hierarchy);
        }

        private static void UpdateLastSeenProjectDirectory(IVsHierarchy hierarchy)
        {
            VisualHgSolution.LastSeenProjectDirectory = VisualHgSolution.GetDirectoryName(hierarchy);
        }

        private void OnAfterOpenSolution()
        {
            if (!Active && Configuration.Global.AutoActivatePlugin)
            {
                var root = VisualHgSolution.SolutionRootDirectory;
                
                if (!String.IsNullOrEmpty(root))
                {
                    var rscp = Package.GetGlobalService(typeof(IVsRegisterScciProvider)) as IVsRegisterScciProvider;
                    
                    rscp.RegisterSourceControlProvider(Guids.ProviderGuid);
                }
            }
        }

        private void OnBeforeCloseProject(IVsHierarchy hierarchy)
        {
            repository.SolutionFiles.Remove(hierarchy);
        }

        private void OnBeforeCloseSolution()
        {
            repository.SolutionFiles.Clear();
        }

        private void OnBeforeUnloadProject(IVsHierarchy hierarchy)
        {
            repository.SolutionFiles.Remove(hierarchy);
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
            if (Configuration.Global.AutoAddNewFiles)
            {
                repository.AddFiles(fileNames);
            }
        }

        private void OnAfterRemoveFiles(string[] fileNames)
        {
            repository.RemoveFiles(fileNames);
        }

        private void OnAfterRenameFiles(string[] fileNames, string[] newFileNames)
        {
            repository.RenameFiles(fileNames, newFileNames);
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

            rgsiGlyphs[0] = GetStateIcon(rgpszFullPaths[0]);

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
            OnBeforeCloseProject(pHierarchy);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            OnBeforeCloseSolution();
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            OnBeforeUnloadProject(pRealHierarchy);
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
    }
}