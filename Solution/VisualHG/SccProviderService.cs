using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using HgLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    [Guid(Guids.Service)]
    public partial class SccProviderService :
        IVsSccProvider,
        IVsSccGlyphs,
        IVsSccManager2,
        IVsSccManagerTooltip,
        IVsSolutionEvents,
        IVsSolutionEvents2,
        IVsUpdateSolutionEvents,
        IVsTrackProjectDocumentsEvents2,
        IVsQueryEditQuerySave2,
        IDisposable
    {
        private bool _nodesGlyphsDirty = true;
        
        private uint _vsSolutionEventsCookie = VSConstants.VSCOOKIE_NIL;
        private uint _trackProjectDocumentsEventsCookie = VSConstants.VSCOOKIE_NIL;
        private uint _buildManagerCookie = VSConstants.VSCOOKIE_NIL;
        
        private DateTime _lastUpdate;

        private SccProvider _sccProvider;


        public bool Active { get; private set; }

        public VisualHgRepository Repository { get; private set; }


        public SccProviderService(SccProvider sccProvider)
        {
            Debug.Assert(sccProvider != null);

            _sccProvider = sccProvider;

            Repository = new VisualHgRepository();
            Repository.StatusChanged += SetNodesGlyphsDirty;

            var solution = _sccProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            solution.AdviseSolutionEvents(this, out _vsSolutionEventsCookie);
            Debug.Assert(_vsSolutionEventsCookie != VSConstants.VSCOOKIE_NIL);
            
            var trackProjectDocuments = _sccProvider.GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
            trackProjectDocuments.AdviseTrackProjectDocumentsEvents(this, out _trackProjectDocumentsEventsCookie);
            Debug.Assert(_trackProjectDocumentsEventsCookie != VSConstants.VSCOOKIE_NIL);

            var buildManager = _sccProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
            buildManager.AdviseUpdateSolutionEvents(this, out _buildManagerCookie);
            Debug.Assert(_buildManagerCookie != VSConstants.VSCOOKIE_NIL);
        }


        public void Dispose()
        {
            Repository.StatusChanged -= SetNodesGlyphsDirty;
            Repository.Dispose();

            statusImageList.Dispose();
            
            if (_vsSolutionEventsCookie != VSConstants.VSCOOKIE_NIL)
            {
                var solution = _sccProvider.GetService(typeof(SVsSolution)) as IVsSolution;
                solution.UnadviseSolutionEvents(_vsSolutionEventsCookie);
                _vsSolutionEventsCookie = VSConstants.VSCOOKIE_NIL;
            }

            if (_trackProjectDocumentsEventsCookie != VSConstants.VSCOOKIE_NIL)
            {
                var trackProjectDocuments = _sccProvider.GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
                trackProjectDocuments.UnadviseTrackProjectDocumentsEvents(_trackProjectDocumentsEventsCookie);
                _trackProjectDocumentsEventsCookie = VSConstants.VSCOOKIE_NIL;
            }

            if (_buildManagerCookie != VSConstants.VSCOOKIE_NIL)
            {
                var buildManager = _sccProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
                buildManager.UnadviseUpdateSolutionEvents(_buildManagerCookie);
            }
        }


        public int SetActive()
        {
            Active = true;

            var solution = _sccProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            
            Repository.UpdateSolution(solution);

            return VSConstants.S_OK;
        }

        public int SetInactive()
        {
            Active = false;
            return VSConstants.S_OK;
        }

        public int AnyItemsUnderSourceControl(out int pfResult)
        {
            pfResult = Active && !Repository.IsEmpty ? 1 : 0;

            return VSConstants.S_OK;
        }

        public int BeginQuerySaveBatch()
        {
            return VSConstants.S_OK;
        }

        public int EndQuerySaveBatch()
        {
            return VSConstants.S_OK;
        }

        public int DeclareReloadableFile(string pszMkDocument, uint rgf, VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        public int DeclareUnreloadableFile(string pszMkDocument, uint rgf, VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        public int IsReloadable(string pszMkDocument, out int pbResult)
        {
            // Since we're not tracking which files are reloadable and which not, consider everything reloadable
            pbResult = 1;
            return VSConstants.S_OK;
        }

        public int OnAfterSaveUnreloadableFile(string pszMkDocument, uint rgf, VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        public int QueryEditFiles(uint rgfQueryEdit, int cFiles, string[] rgpszMkDocuments, uint[] rgrgf, VSQEQS_FILE_ATTRIBUTE_DATA[] rgFileInfo, out uint pfEditVerdict, out uint prgfMoreInfo)
        {
            pfEditVerdict = (uint)tagVSQueryEditResult.QER_EditOK;
            prgfMoreInfo = 0;
            return VSConstants.S_OK;
        }

        public int QuerySaveFile(string pszMkDocument, uint rgf, VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo, out uint pdwQSResult)
        {
            if (Active && File.Exists(pszMkDocument))
            {
                try
                {
                    var attribures = File.GetAttributes(pszMkDocument);

                    if ((attribures & FileAttributes.ReadOnly) > 0)
                    {
                        File.SetAttributes(pszMkDocument, (attribures & ~FileAttributes.ReadOnly));
                    }

                    Repository.UpdateFileStatus(pszMkDocument);
                }
                catch { }
            }

            pdwQSResult = (uint)tagVSQuerySaveResult.QSR_SaveOK;

            return VSConstants.S_OK;
        }

        public int QuerySaveFiles(uint rgfQuerySave, int cFiles, string[] rgpszMkDocuments, uint[] rgrgf, VSQEQS_FILE_ATTRIBUTE_DATA[] rgFileInfo, out uint pdwQSResult)
        {
            Repository.UpdateFileStatus(rgpszMkDocuments);

            pdwQSResult = (uint)tagVSQuerySaveResult.QSR_SaveOK;

            return VSConstants.S_OK;
        }


        public HgFileStatus GetFileStatus(string filename)
        {
            return Repository.GetFileStatus(filename);
        }


        public void UpdateDirtyNodesGlyphs(object sender, EventArgs e)
        {
            if (_nodesGlyphsDirty && (DateTime.Now - _lastUpdate).Milliseconds > 100)
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

            _lastUpdate = DateTime.Now;
            _nodesGlyphsDirty = false;
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


        private void SetNodesGlyphsDirty(object sender, EventArgs e)
        {
            _nodesGlyphsDirty = true;
        }


        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int UpdateSolution_Cancel()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            Repository.SolutionBuildEnded();
            return VSConstants.E_NOTIMPL;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            Repository.SolutionBuildStarted();
            return VSConstants.E_NOTIMPL;
        }
    }
}