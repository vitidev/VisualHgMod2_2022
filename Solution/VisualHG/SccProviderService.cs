using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Windows.Forms;


namespace VisualHG
{
    [Guid(GuidList.ProviderServiceGuid)]
    public class SccProviderService :
        IVsSccProvider,             // Required for provider registration with source control manager
        IVsSccManager2,             // Base source control functionality interface
        IVsSccManagerTooltip,       // Provide tooltips for source control items
        IVsSolutionEvents,          // We'll register for solution events, these are usefull for source control
        IVsSolutionEvents2,
        IVsQueryEditQuerySave2,     // Required to allow editing of controlled files 
        IVsTrackProjectDocumentsEvents2,  // Usefull to track project changes (add, renames, deletes, etc)
        IVsSccGlyphs,
        IVsUpdateSolutionEvents,
        IDisposable
    {
        // Whether the provider is active or not
        private bool _active = false;
        // The service and source control provider
        private SccProvider _sccProvider = null;
        // The cookie for solution events 
        private uint _vsSolutionEventsCookie;
        // The cookie for project document events
        private uint _tpdTrackProjectDocumentsCookie;
        // solution file status cache
        HGStatusTracker _sccStatusTracker = new HGStatusTracker();
        // service.advise IVsUpdateSolutionEvents cooky
        uint _dwBuildManagerCooky = 0;

        // DirtyNodesGlyphs update flag
        bool _bNodesGlyphsDirty = true;

        // remember the latest OnQueryRemoveDirectories remove list
        //string[] _RemoveDirectoriesQueue = null;

        #region SccProvider Service initialization/unitialization

        public SccProviderService(SccProvider sccProvider)
        {
            Debug.Assert(null != sccProvider);
            _sccProvider = sccProvider;

            // Subscribe to solution events
            IVsSolution sol = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));
            sol.AdviseSolutionEvents(this, out _vsSolutionEventsCookie);
            Debug.Assert(VSConstants.VSCOOKIE_NIL != _vsSolutionEventsCookie);

            // Subscribe to project documents
            IVsTrackProjectDocuments2 tpdService = (IVsTrackProjectDocuments2)_sccProvider.GetService(typeof(SVsTrackProjectDocuments));
            tpdService.AdviseTrackProjectDocumentsEvents(this, out _tpdTrackProjectDocumentsCookie);
            Debug.Assert(VSConstants.VSCOOKIE_NIL != _tpdTrackProjectDocumentsCookie);

            // Subscribe to status events
            _sccStatusTracker.HGStatusChanged += new HGLib.HGStatusChangedEvent(SetNodesGlyphsDirty);

            IVsSolutionBuildManager buildManagerService = _sccProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
            buildManagerService.AdviseUpdateSolutionEvents(this, out _dwBuildManagerCooky);
        }

        public void Dispose()
        {
            // Unregister from receiving solution events
            if (VSConstants.VSCOOKIE_NIL != _vsSolutionEventsCookie)
            {
                IVsSolution sol = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));
                sol.UnadviseSolutionEvents(_vsSolutionEventsCookie);
                _vsSolutionEventsCookie = VSConstants.VSCOOKIE_NIL;
            }

            // Unregister from receiving project documents
            if (VSConstants.VSCOOKIE_NIL != _tpdTrackProjectDocumentsCookie)
            {
                IVsTrackProjectDocuments2 tpdService = (IVsTrackProjectDocuments2)_sccProvider.GetService(typeof(SVsTrackProjectDocuments));
                tpdService.UnadviseTrackProjectDocumentsEvents(_tpdTrackProjectDocumentsCookie);
                _tpdTrackProjectDocumentsCookie = VSConstants.VSCOOKIE_NIL;
            }

            // Unregister from storrage events
            _sccStatusTracker.HGStatusChanged -= new HGLib.HGStatusChangedEvent(SetNodesGlyphsDirty);

            IVsSolutionBuildManager buildManagerService = _sccProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
            buildManagerService.UnadviseUpdateSolutionEvents(_dwBuildManagerCooky);
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsSccProvider specific functions
        //--------------------------------------------------------------------------------
        #region IVsSccProvider interface functions

        // Called by the scc manager when the provider is activated. 
        // Make visible and enable if necessary scc related menu commands
        public int SetActive()
        {
            Trace.WriteLine("SetActive");
            
            _active = true;

            // add all projects of this solution to the status file cache
            IVsSolution solution = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));
            this._sccStatusTracker.UpdateProjects(solution);
            this._sccStatusTracker.SetCacheDirty();

            return VSConstants.S_OK;
        }

        // Called by the scc manager when the provider is deactivated. 
        // Hides and disable scc related menu commands
        public int SetInactive()
        {
            Trace.WriteLine("SetInactive"); 
            
            _active = false;
            return VSConstants.S_OK;
        }

        public int AnyItemsUnderSourceControl(out int pfResult)
        {
            if (!_active)
            {
                pfResult = 0;
            }
            else
            {
                // Although the parameter is an int, it's in reality a BOOL value, so let's return 0/1 values
                pfResult = _sccStatusTracker.AnyItemsUnderSourceControl() ? 1 : 0;
            }

            return VSConstants.S_OK;
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsSccManager2 specific functions
        //--------------------------------------------------------------------------------
        #region IVsSccManager2 interface functions

        public int BrowseForProject(out string pbstrDirectory, out int pfOK)
        {
            // Obsolete method
            pbstrDirectory = null;
            pfOK = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int CancelAfterBrowseForProject()
        {
            // Obsolete method
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Returns whether the source control provider is fully installed
        /// </summary>
        public int IsInstalled(out int pbInstalled)
        {
            // All source control packages should always return S_OK and set pbInstalled to nonzero
            pbInstalled = 1;
            return VSConstants.S_OK;
        }

        StatusImageMapper _statusImages = new StatusImageMapper();
        uint _baseIndex;
        ImageList _glyphList;
        /// <summary>
        /// Called by the IDE to get a custom glyph image list for source control status.
        /// </summary>
        /// <param name="BaseIndex">[in] Value to add when returning glyph index.</param>
        /// <param name="pdwImageListHandle">[out] Handle to the custom image list.</param>
        /// <returns>handle of an image list</returns>
        public int GetCustomGlyphList(uint BaseIndex, out uint pdwImageListHandle)
        {
            // We give VS all our custom glyphs from baseindex upwards
            if (_glyphList == null)
            {
                _baseIndex = BaseIndex;
                _glyphList = _statusImages.CreateStatusImageList();
            }
            pdwImageListHandle = unchecked((uint)_glyphList.Handle);

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Provide source control icons for the specified files and returns scc status of files
        /// </summary>
        /// <returns>The method returns S_OK if at least one of the files is controlled, S_FALSE if none of them are</returns>
        public int GetSccGlyph([InAttribute] int cFiles, [InAttribute] string[] rgpszFullPaths, [OutAttribute] VsStateIcon[] rgsiGlyphs, [OutAttribute] uint[] rgdwSccStatus)
        {
            if (rgpszFullPaths[0] == null)
                return 0;

            // Return the icons and the status. While the status is a combination a flags, we'll return just values 
            // with one bit set, to make life easier for GetSccGlyphsFromStatus
            HGLib.SourceControlStatus status = _sccStatusTracker.GetFileStatus(rgpszFullPaths[0]);
            switch (status)
            {
                // STATEICON_CHECKEDIN schloss
                // STATEICON_CHECKEDOUT roter haken
                // STATEICON_CHECKEDOUTEXCLUSIVE roter haken
                // STATEICON_CHECKEDOUTSHAREDOTHER männchen
                // STATEICON_DISABLED roter ring / durchgestrichen
                //  STATEICON_EDITABLE bleistift
                // STATEICON_EXCLUDEDFROMSCC einbahnstrasse
                // STATEICON_MAXINDEX nix
                // STATEICON_NOSTATEICON nix
                // STATEICON_ORPHANED blaue flagge
                // STATEICON_READONLY schloss

                // my states
                case HGLib.SourceControlStatus.scsControlled:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 0);
                    break;

                case HGLib.SourceControlStatus.scsModified:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 1);
                    break;

                case HGLib.SourceControlStatus.scsAdded:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 2);
                    break;

                case HGLib.SourceControlStatus.scsRenamed:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 3);
                    break;

                case HGLib.SourceControlStatus.scsRemoved:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 1);
                    break;

                case HGLib.SourceControlStatus.scsIgnored:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_BLANK;
                    break;

                case HGLib.SourceControlStatus.scsUncontrolled:
                    rgsiGlyphs[0] = 0;// (VsStateIcon)(_baseIndex + 3); 
                    break;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines the corresponding scc status glyph to display, given a combination of scc status flags
        /// </summary>
        public int GetSccGlyphFromStatus([InAttribute] uint dwSccStatus, [OutAttribute] VsStateIcon[] psiGlyph)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// One of the most important methods in a source control provider,
        /// is called by projects that are under source control when they are first
        /// opened to register project settings
        /// </summary>
        public int RegisterSccProject([InAttribute] IVsSccProject2 pscp2Project, [InAttribute] string pszSccProjectName, [InAttribute] string pszSccAuxPath, [InAttribute] string pszSccLocalPath, [InAttribute] string pszProvider)
        {
            Trace.WriteLine("RegisterSccProject"); 
            
            if (pscp2Project != null)
            {
                _sccStatusTracker.UpdateProject(pscp2Project);
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by projects registered with the source control portion of the environment
        /// before they are closed. 
        /// </summary>
        public int UnregisterSccProject([InAttribute] IVsSccProject2 pscp2Project)
        {
            return VSConstants.S_OK;
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsSccManagerTooltip specific functions
        //--------------------------------------------------------------------------------
        #region IVsSccManagerTooltip interface functions

        /// <summary>
        /// Called by solution explorer to provide tooltips for items. Returns a text describing the source control status of the item.
        /// </summary>
        public int GetGlyphTipText([InAttribute] IVsHierarchy phierHierarchy, [InAttribute] uint itemidNode, out string pbstrTooltipText)
        {
            // Initialize output parameters
            pbstrTooltipText = "";

            IList<string> files = SccProvider.GetNodeFiles(phierHierarchy, itemidNode);
            if (files.Count == 0)
            {
                return VSConstants.S_OK;
            }

            // Return the glyph text based on the first file of node (the master file)
            HGLib.SourceControlStatus status = _sccStatusTracker.GetFileStatus(files[0]);
            switch (status)
            {
              // my states
              case HGLib.SourceControlStatus.scsControlled:
                pbstrTooltipText = "Clean";
                break;

              case HGLib.SourceControlStatus.scsModified:
                pbstrTooltipText = "Modified";
                break;

              case HGLib.SourceControlStatus.scsAdded:
                pbstrTooltipText = "Added";
                break;

              case HGLib.SourceControlStatus.scsRenamed:
                pbstrTooltipText = "Renamed";
                break;

              case HGLib.SourceControlStatus.scsRemoved:
                pbstrTooltipText = "Removed";
                break;

              case HGLib.SourceControlStatus.scsIgnored:
                pbstrTooltipText = "Ignored";
                break;

              case HGLib.SourceControlStatus.scsUncontrolled:
                pbstrTooltipText = "Uncontrolled";
                break;

                default:
                    pbstrTooltipText = "";
                    break;
            }

            return VSConstants.S_OK;
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsSolutionEvents and IVsSolutionEvents2 specific functions
        //--------------------------------------------------------------------------------
        #region IVsSolutionEvents interface functions

        public int OnAfterOpenSolution([InAttribute] Object pUnkReserved, [InAttribute] int fNewSolution)
        {
            Trace.WriteLine("OnAfterOpenSolution");
            
            if (!Active)
            {
                /*string root = HGLib.HG.LookupRootDirectory(_sccProvider.GetSolutionFileName());
                if (root.Length > 0)
                {
                    IVsRegisterScciProvider rscp = (IVsRegisterScciProvider)_sccProvider.GetService(typeof(IVsRegisterScciProvider));
                    if (rscp != null)
                    {
                        rscp.RegisterSourceControlProvider(GuidList.guidSccProvider);
                    }
                }
                */
            }
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution([InAttribute] Object pUnkReserved)
        {
            Trace.WriteLine("OnAfterCloseSolution");

            _sccStatusTracker.ClearStatusCache();
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject([InAttribute] IVsHierarchy pStubHierarchy, [InAttribute] IVsHierarchy pRealHierarchy)
        {
            Trace.WriteLine("OnAfterLoadProject");

            _sccStatusTracker.UpdateProject(pRealHierarchy as IVsSccProject2);
            _sccProvider._LastSeenProjectDir = SccProjectData.ProjectDirectory(pRealHierarchy);

            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject([InAttribute] IVsHierarchy pHierarchy, [InAttribute] int fAdded)
        {
            Trace.WriteLine("OnAfterOpenProject");

            if (fAdded == 1)
            {
                IList<string> fileList = SccProvider.GetProjectFiles(pHierarchy as IVsSccProject2);
                string[] files = new string[fileList.Count];
                fileList.CopyTo(files, 0);
                // add only files wich are not ignored
                _sccStatusTracker.AddNotIgnoredFiles(files);
                _sccProvider._LastSeenProjectDir = SccProjectData.ProjectDirectory(pHierarchy);
            }

            _sccStatusTracker.UpdateProject(pHierarchy as IVsSccProject2);
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject([InAttribute] IVsHierarchy pHierarchy, [InAttribute] int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution([InAttribute] Object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject([InAttribute] IVsHierarchy pRealHierarchy, [InAttribute] IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject([InAttribute] IVsHierarchy pHierarchy, [InAttribute] int fRemoving, [InAttribute] ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution([InAttribute] Object pUnkReserved, [InAttribute] ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject([InAttribute] IVsHierarchy pRealHierarchy, [InAttribute] ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterMergeSolution([InAttribute] Object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsQueryEditQuerySave2 specific functions
        //--------------------------------------------------------------------------------
        #region IVsQueryEditQuerySave2 interface functions

        public int BeginQuerySaveBatch()
        {
            return VSConstants.S_OK;
        }

        public int EndQuerySaveBatch()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// States that a file will be reloaded if it changes on disk.
        /// </summary>
        /// <param name="pszMkDocument">The PSZ mk document.</param>
        /// <param name="rgf">The RGF.</param>
        /// <param name="pFileInfo">The p file info.</param>
        /// <returns></returns>
        public int DeclareReloadableFile([InAttribute] string pszMkDocument, [InAttribute] uint rgf, [InAttribute] VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// States that a file will not be reloaded if it changes on disk
        /// </summary>
        /// <param name="pszMkDocument">The PSZ mk document.</param>
        /// <param name="rgf">The RGF.</param>
        /// <param name="pFileInfo">The p file info.</param>
        /// <returns></returns>
        public int DeclareUnreloadableFile([InAttribute] string pszMkDocument, [InAttribute] uint rgf, [InAttribute] VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        public int IsReloadable([InAttribute] string pszMkDocument, out int pbResult)
        {
            // Since we're not tracking which files are reloadable and which not, consider everything reloadable
            pbResult = 1;
            return VSConstants.S_OK;
        }

        public int OnAfterSaveUnreloadableFile([InAttribute] string pszMkDocument, [InAttribute] uint rgf, [InAttribute] VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by projects and editors before modifying a file
        /// </summary>
        public int QueryEditFiles([InAttribute] uint rgfQueryEdit, [InAttribute] int cFiles, [InAttribute] string[] rgpszMkDocuments, [InAttribute] uint[] rgrgf, [InAttribute] VSQEQS_FILE_ATTRIBUTE_DATA[] rgFileInfo, out uint pfEditVerdict, out uint prgfMoreInfo)
        {
            // Initialize output variables
            pfEditVerdict = (uint)tagVSQueryEditResult.QER_EditOK;
            prgfMoreInfo = 0;
            return VSConstants.S_OK;
        }
        /// <summary>
        /// Called by editors and projects before saving the files
        /// </summary>
        public int QuerySaveFile([InAttribute] string pszMkDocument, [InAttribute] uint rgf, [InAttribute] VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo, out uint pdwQSResult)
        {
            Trace.WriteLine("QuerySaveFile");
            Trace.WriteLine("    dir: " + pszMkDocument);

            pdwQSResult = (uint)tagVSQuerySaveResult.QSR_SaveOK;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by editors and projects before saving the files
        /// </summary>
        public int QuerySaveFiles([InAttribute] uint rgfQuerySave, [InAttribute] int cFiles, [InAttribute] string[] rgpszMkDocuments, [InAttribute] uint[] rgrgf, [InAttribute] VSQEQS_FILE_ATTRIBUTE_DATA[] rgFileInfo, out uint pdwQSResult)
        {
            Trace.WriteLine("QuerySaveFiles");
            for (int iFile = 0; iFile < cFiles; ++iFile)
            {
                Trace.WriteLine("    dir: " + rgpszMkDocuments[iFile] );
            }

            pdwQSResult = (uint)tagVSQuerySaveResult.QSR_SaveOK;
            return VSConstants.S_OK;
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsTrackProjectDocumentsEvents2 specific functions
        //--------------------------------------------------------------------------------
        #region IVsTrackProjectDocumentsEvents2 interface funcions

        public int OnQueryAddFiles([InAttribute] IVsProject pProject, [InAttribute] int cFiles, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSQUERYADDFILEFLAGS[] rgFlags, [OutAttribute] VSQUERYADDFILERESULTS[] pSummaryResult, [OutAttribute] VSQUERYADDFILERESULTS[] rgResults)
        {
            _sccStatusTracker.EnableRaisingEvents(false); 
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Implement this function to update the project scc glyphs when the items are added to the project.
        /// If a project doesn't call GetSccGlyphs as they should do (as solution folder do), this will update correctly the glyphs when the project is controled
        /// </summary>
        public int OnAfterAddFilesEx([InAttribute] int cProjects, [InAttribute] int cFiles, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSADDFILEFLAGS[] rgFlags)
        {
            HGLib.HGFileStatusInfo info;
            _sccStatusTracker.GetFileStatusInfo(rgpszMkDocuments[0], out info);
            if (info == null || info.state == '?')
            {
                _sccStatusTracker.AddNotIgnoredFiles(rgpszMkDocuments);
            }
            _sccStatusTracker.EnableRaisingEvents(true);
            return VSConstants.S_OK;
        }

        public int OnQueryAddDirectories([InAttribute] IVsProject pProject, [InAttribute] int cDirectories, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSQUERYADDDIRECTORYFLAGS[] rgFlags, [OutAttribute] VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, [OutAttribute] VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            Trace.WriteLine("OnQueryAddDirectories");
            for (int iDirectory = 0; iDirectory < cDirectories; ++iDirectory)
            {
                Trace.WriteLine("    dir: " + rgpszMkDocuments[iDirectory] + ", flag: " + rgFlags[iDirectory].ToString());
            }

            return VSConstants.S_OK;
        }

        public int OnAfterAddDirectoriesEx([InAttribute] int cProjects, [InAttribute] int cDirectories, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSADDDIRECTORYFLAGS[] rgFlags)
        {
            Trace.WriteLine("OnAfterAddDirectoriesEx");
            for (int iDirectory = 0; iDirectory < cDirectories; ++iDirectory)
            {
                Trace.WriteLine("    dir: " + rgpszMkDocuments[iDirectory] + ", flag: " + rgFlags[iDirectory].ToString());
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Implement OnQueryRemoveFilesevent to warn the user when he's deleting controlled files.
        /// The user gets the chance to cancel the file removal.
        /// </summary>
        public int OnQueryRemoveFiles([InAttribute] IVsProject pProject, [InAttribute] int cFiles, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSQUERYREMOVEFILEFLAGS[] rgFlags, [OutAttribute] VSQUERYREMOVEFILERESULTS[] pSummaryResult, [OutAttribute] VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            _sccStatusTracker.EnableRaisingEvents(false);
            return VSConstants.S_OK;
        }
        
        public int OnAfterRemoveFiles([InAttribute] int cProjects, [InAttribute] int cFiles, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSREMOVEFILEFLAGS[] rgFlags)
        {
            StoreSolution();
            
            _sccStatusTracker.EnableRaisingEvents(true);

            if (rgpProjects == null || rgpszMkDocuments == null)
                return VSConstants.E_POINTER;

            if (!File.Exists(rgpszMkDocuments[0])) // PropagateFilesRemoved only if the file was actually removed
                _sccStatusTracker.PropagateFilesRemoved(rgpszMkDocuments);
                
            return VSConstants.S_OK;
        }

        public int OnQueryRemoveDirectories([InAttribute] IVsProject pProject, [InAttribute] int cDirectories, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, [OutAttribute] VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, [OutAttribute] VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        public void StoreSolution()
        {
            //TODO store project and solution files to disk
            //IVsSolution solution = (IVsSolution)_sccProvider.GetService(typeof(IVsSolution)); 
            //solution.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, null, 0);
        }
        
        public int OnAfterRemoveDirectories([InAttribute] int cProjects, [InAttribute] int cDirectories, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            //StoreSolution();
            return VSConstants.S_OK;
        }

        public int OnQueryRenameFiles([InAttribute] IVsProject pProject, [InAttribute] int cFiles, [InAttribute] string[] rgszMkOldNames, [InAttribute] string[] rgszMkNewNames, [InAttribute] VSQUERYRENAMEFILEFLAGS[] rgFlags, [OutAttribute] VSQUERYRENAMEFILERESULTS[] pSummaryResult, [OutAttribute] VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            _sccStatusTracker.EnableRaisingEvents(false); 
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Implement OnAfterRenameFiles event to rename a file in the source control store when it gets renamed in the project
        /// Also, rename the store if the project itself is renamed
        /// </summary>
        public int OnAfterRenameFiles([InAttribute] int cProjects, [InAttribute] int cFiles, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgszMkOldNames, [InAttribute] string[] rgszMkNewNames, [InAttribute] VSRENAMEFILEFLAGS[] rgFlags)
        {
            _sccStatusTracker.EnableRaisingEvents(true); 
            _sccStatusTracker.PropagateFileRenamed(rgszMkOldNames, rgszMkNewNames);
            StoreSolution();            
            return VSConstants.S_OK;
        }

        public int OnQueryRenameDirectories([InAttribute] IVsProject pProject, [InAttribute] int cDirs, [InAttribute] string[] rgszMkOldNames, [InAttribute] string[] rgszMkNewNames, [InAttribute] VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, [OutAttribute] VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, [OutAttribute] VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterRenameDirectories([InAttribute] int cProjects, [InAttribute] int cDirs, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgszMkOldNames, [InAttribute] string[] rgszMkNewNames, [InAttribute] VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            StoreSolution(); 
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterSccStatusChanged([InAttribute] int cProjects, [InAttribute] int cFiles, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] uint[] rgdwSccStatus)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion  // IVsTrackProjectDocumentsEvents2 interface funcions


        #region Files and Project Management Functions

        /// <summary>
        /// Returns whether this source control provider is the active scc provider.
        /// </summary>
        public bool Active
        {
            get { return _active; }
        }

        /// <summary>
        /// Checks whether the specified project or solution (pHier==null) is under source control
        /// </summary>
        /// <returns>True if project is controlled.</returns>
        public bool IsProjectControlled(IVsHierarchy pHier)
        {
            return _sccStatusTracker.AnyItemsUnderSourceControl();
        }

        /// <summary>
        /// set the node glyphs dirty flag to true
        /// </summary>
        public void SetNodesGlyphsDirty()
        {
            _bNodesGlyphsDirty = true;
        }

        /// <summary>
        /// call RefreshNodesGlyphs to update all Glyphs 
        /// if the _bNodesGlyphsDirty is true
        /// </summary>
        public void UpdateDirtyNodesGlyphs()
        {
            if (_bNodesGlyphsDirty)
                RefreshNodesGlyphs();

            _bNodesGlyphsDirty = false;
        }

        public void RefreshNodesGlyphs()
        {
            var solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
            var projectList = _sccProvider.GetLoadedControllableProjects();

            // We'll also need to refresh the solution folders glyphs
            // to reflect the controlled state
            IList<VSITEMSELECTION> nodes = new List<VSITEMSELECTION>();

            { // add solution root item
                VSITEMSELECTION vsItem;
                vsItem.itemid = VSConstants.VSITEMID_ROOT;
                vsItem.pHier = solHier;// pHierarchy;
                nodes.Add(vsItem);
            }

            // add project node items
            foreach (IVsHierarchy hr in projectList)
            {
                VSITEMSELECTION vsItem;
                vsItem.itemid = VSConstants.VSITEMID_ROOT;
                vsItem.pHier = hr;
                nodes.Add(vsItem);
            }

            _sccProvider.RefreshNodesGlyphs(nodes);
        }

        #endregion

        #region IVsUpdateSolutionEvents Members

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
            Trace.WriteLine("UpdateSolution_Done"); 
            
            _sccStatusTracker.UpdateSolution_Done();
            return VSConstants.E_NOTIMPL;

        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            Trace.WriteLine("UpdateSolution_StartUpdate"); 

            _sccStatusTracker.UpdateSolution_StartUpdate();
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}