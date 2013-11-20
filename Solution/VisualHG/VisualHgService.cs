using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HgLib;
using Microsoft.VisualStudio;
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

        private SccProvider _sccProvider;


        public bool Active
        {
            get { return _active; }
            set
            {
                if (value && !_active)
                {
                    var solution = _sccProvider.GetService(typeof(SVsSolution)) as IVsSolution;

                    Repository.UpdateSolution(solution);
                }

                _active = value;
            }
        }

        public VisualHgRepository Repository { get; private set; }


        public VisualHgService(SccProvider sccProvider)
        {
            Debug.Assert(sccProvider != null);

            _sccProvider = sccProvider;

            Repository = new VisualHgRepository();
            Repository.StatusChanged += SetNodesGlyphsDirty;

            var solution = _sccProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            solution.AdviseSolutionEvents(this, out vsSolutionEventsCookie);
            Debug.Assert(vsSolutionEventsCookie != VSConstants.VSCOOKIE_NIL);

            var trackProjectDocuments = _sccProvider.GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
            trackProjectDocuments.AdviseTrackProjectDocumentsEvents(this, out trackProjectDocumentsEventsCookie);
            Debug.Assert(trackProjectDocumentsEventsCookie != VSConstants.VSCOOKIE_NIL);

            var buildManager = _sccProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
            buildManager.AdviseUpdateSolutionEvents(this, out buildManagerCookie);
            Debug.Assert(buildManagerCookie != VSConstants.VSCOOKIE_NIL);
        }

        public void Dispose()
        {
            Repository.StatusChanged -= SetNodesGlyphsDirty;
            Repository.Dispose();

            statusImageList.Dispose();

            if (vsSolutionEventsCookie != VSConstants.VSCOOKIE_NIL)
            {
                var solution = _sccProvider.GetService(typeof(SVsSolution)) as IVsSolution;
                solution.UnadviseSolutionEvents(vsSolutionEventsCookie);
                vsSolutionEventsCookie = VSConstants.VSCOOKIE_NIL;
            }

            if (trackProjectDocumentsEventsCookie != VSConstants.VSCOOKIE_NIL)
            {
                var trackProjectDocuments = _sccProvider.GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
                trackProjectDocuments.UnadviseTrackProjectDocumentsEvents(trackProjectDocumentsEventsCookie);
                trackProjectDocumentsEventsCookie = VSConstants.VSCOOKIE_NIL;
            }

            if (buildManagerCookie != VSConstants.VSCOOKIE_NIL)
            {
                var buildManager = _sccProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
                buildManager.UnadviseUpdateSolutionEvents(buildManagerCookie);
            }
        }

        
        public HgFileStatus GetFileStatus(string filename)
        {
            return Repository.GetFileStatus(filename);
        }


        private void SetNodesGlyphsDirty(object sender, EventArgs e)
        {
            nodesGlyphsDirty = true;
        }

        public void UpdateDirtyNodesGlyphs(object sender, EventArgs e)
        {
            if (nodesGlyphsDirty && (DateTime.Now - lastUpdate).Milliseconds > 100)
            {
                RefreshNodesGlyphs();
                UpdateMainWindowTitle();
                UpdatePendingChangesToolWindow();
            }
        }

        private void RefreshNodesGlyphs()
        {
            var nodes = new [] { GetSolutionVsItemSelection() }
                .Concat(_sccProvider.LoadedProjects.Select(GetVsItemSelection))
                .ToArray();

            _sccProvider.UpdateGlyphs(nodes);

            lastUpdate = DateTime.Now;
            nodesGlyphsDirty = false;
        }

        private VSITEMSELECTION GetSolutionVsItemSelection()
        {
            var hierarchy = _sccProvider.GetService(typeof(SVsSolution)) as IVsHierarchy;

            return GetVsItemSelection(hierarchy);
        }

        private VSITEMSELECTION GetVsItemSelection(IVsHierarchy hierarchy)
        {
            return new VSITEMSELECTION {
                itemid = VSConstants.VSITEMID_ROOT,
                pHier = hierarchy,
            };
        }

        private void UpdatePendingChangesToolWindow()
        {
            _sccProvider.UpdatePendingChangesToolWindow();
        }

        private void UpdateMainWindowTitle()
        {
            var branches = Repository.Branches;
            var text = branches.Length > 0 ? branches.Distinct().Aggregate((x, y) => String.Concat(x, ", ", y)) : "";

            _sccProvider.UpdateMainWindowCaption(text);
        }


        private bool AnyItemsUnderSourceControl
        {
            get { return Active && !Repository.IsEmpty; }
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
            var status = Repository.GetFileStatus(fileName);
            var iconIndex = ImageMapper.GetStatusIconIndex(status);

            return (VsStateIcon)(iconBaseIndex + iconIndex);
        }

        private void OnProjectRegister(IVsSccProject2 project)
        {
            if (project != null)
            {
                Repository.UpdateProject(project);
            }
        }

        private string GetToolTipText(IVsHierarchy hierarchy, uint itemId)
        {
            var files = SccProvider.GetItemFiles(hierarchy, itemId);
            
            if (files.Length == 0)
            {
                return "";
            }

            var fileName = files[0];
            
            var text = Repository.GetFileStatus(fileName).ToString();
            var branch = Repository.GetBranch(fileName);

            if (!String.IsNullOrEmpty(branch))
            {
                text += " (" + branch + ")";
            }

            return text;
        }


        private void OnAfterCloseSolution()
        {
            _sccProvider.LastSeenProjectDirectory = "";

            Repository.Clear();
            UpdatePendingChangesToolWindow();
        }

        private void OnAfterLoadProject(IVsHierarchy hierarchy)
        {
            _sccProvider.LastSeenProjectDirectory = SccProvider.GetDirectoryName(hierarchy);

            var project = hierarchy as IVsSccProject2;

            if (project != null)
            {
                Repository.UpdateProject(project);
            }
        }

        private void OnAfterOpenProject(IVsHierarchy hierarchy)
        {
            var project = hierarchy as IVsSccProject2;
                        
            var files = Repository.SolutionFiles.Add(hierarchy);

            foreach (var root in files.Select(x => HgPath.FindRepositoryRoot(x)).Distinct())
            {
                Repository.UpdateRootStatus(root);
            }

            if (Configuration.Global.AddFilesOnLoad)
            {
                Repository.AddFiles(files);
            }
            
            _sccProvider.LastSeenProjectDirectory = SccProvider.GetDirectoryName(hierarchy);
        }

        private void OnAfterOpenSolution()
        {
            if (!Active && Configuration.Global.AutoActivatePlugin)
            {
                var root = _sccProvider.SolutionRootDirectory;
                
                if (!String.IsNullOrEmpty(root))
                {
                    var rscp = _sccProvider.GetService(typeof(IVsRegisterScciProvider)) as IVsRegisterScciProvider;
                    
                    rscp.RegisterSourceControlProvider(Guids.ProviderGuid);
                }
            }
        }

        private void OnBeforeCloseProject(IVsHierarchy hierarchy)
        {
            Repository.SolutionFiles.Remove(hierarchy);
        }

        private void OnBeforeCloseSolution()
        {
            Repository.SolutionFiles.Clear();
        }

        private void OnBeforeUnloadProject(IVsHierarchy hierarchy)
        {
            Repository.SolutionFiles.Remove(hierarchy);
        }


        private void OnSolutionBuildEnded()
        {
            Repository.SolutionBuildEnded();
        }

        private void OnSolutionBuildStarted()
        {
            Repository.SolutionBuildStarted();
        }

        private void OnFileSave(params string[] fileNames)
        {
            if (Active)
            {
                Repository.UpdateFileStatus(fileNames);
            }
        }

        
        private void OnAfterAddFiles(string[] fileNames)
        {
            if (Configuration.Global.AutoAddNewFiles)
            {
                Repository.AddFiles(fileNames);
            }
        }

        private void OnAfterRemoveFiles(string[] fileNames)
        {
            Repository.RemoveFiles(fileNames);
        }

        private void OnAfterRenameFiles(string[] fileNames, string[] newFileNames)
        {
            Repository.RenameFiles(fileNames, newFileNames);
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