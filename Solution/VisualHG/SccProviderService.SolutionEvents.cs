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
    public partial class SccProviderService
    {
        //--------------------------------------------------------------------------------
        // IVsSolutionEvents and IVsSolutionEvents2 specific functions
        //--------------------------------------------------------------------------------
        #region IVsSolutionEvents interface functions

        public int OnAfterOpenSolution([InAttribute] Object pUnkReserved, [InAttribute] int fNewSolution)
        {
            Trace.WriteLine("OnAfterOpenSolution");

            // Make VisualHG the active SCC controler on Mercurial solution types
            if (!Active && Configuration.Global._autoActivatePlugin)
            {
                string root = this._sccProvider.GetRootDirectory();
                if (root.Length > 0)
                {
                    IVsRegisterScciProvider rscp = (IVsRegisterScciProvider)this._sccProvider.GetService(typeof(IVsRegisterScciProvider));
                    rscp.RegisterSourceControlProvider(GuidList.guidSccProvider);
                }
            }
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution([InAttribute] Object pUnkReserved)
        {
            Trace.WriteLine("OnAfterCloseSolution");

            _sccStatusTracker.ClearStatusCache();
            _sccProvider._LastSeenProjectDir = string.Empty;
            // update pending tool window
            UpdatePendingWindowState();

            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject([InAttribute] IVsHierarchy pStubHierarchy, [InAttribute] IVsHierarchy pRealHierarchy)
        {
            Trace.WriteLine("OnAfterLoadProject");

            _sccProvider._LastSeenProjectDir = SccProjectData.ProjectDirectory(pRealHierarchy);
            _sccStatusTracker.UpdateProject(pRealHierarchy as IVsSccProject2);
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject([InAttribute] IVsHierarchy pHierarchy, [InAttribute] int fAdded)
        {
            Trace.WriteLine("OnAfterOpenProject");

            //if (fAdded == 1)
            {
                IVsSccProject2 project = pHierarchy as IVsSccProject2;
                
                IList<string> fileList = SccProvider.GetProjectFiles(project);
                _sccStatusTracker.AddFileToProjectCache(fileList, project);

                if (fileList.Count > 0)
                {
                    string[] files = new string[fileList.Count];
                    fileList.CopyTo(files, 0);
                    // add only files wich are not ignored
                    if (Configuration.Global._autoAddFiles)
                        _sccStatusTracker.AddWorkItem(new HGLib.TrackFilesAddedNotIgnored(files));
                    else
                        _sccStatusTracker.AddWorkItem(new HGLib.UpdateFileStatusCommand(files));
                }
            }

            _sccProvider._LastSeenProjectDir = SccProjectData.ProjectDirectory(pHierarchy);
            _sccStatusTracker.UpdateProject(pHierarchy as IVsSccProject2);
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject([InAttribute] IVsHierarchy pHierarchy, [InAttribute] int fRemoved)
        {
            if (_sccStatusTracker.FileProjectMapCacheCount() > 0)
            { 
                IVsSccProject2 project = pHierarchy as IVsSccProject2;
                IList<string> fileList = SccProvider.GetProjectFiles(project);
                _sccStatusTracker.RemoveFileFromProjectCache(fileList);
            }
                        
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution([InAttribute] Object pUnkReserved)
        {
            _sccStatusTracker.ClearFileToProjectCache(); 
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject([InAttribute] IVsHierarchy pRealHierarchy, [InAttribute] IVsHierarchy pStubHierarchy)
        {
            if (_sccStatusTracker.FileProjectMapCacheCount() > 0)
            {
                IVsSccProject2 project = pRealHierarchy as IVsSccProject2;
                IList<string> fileList = SccProvider.GetProjectFiles(project);
                _sccStatusTracker.RemoveFileFromProjectCache(fileList);
            }

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
        // IVsTrackProjectDocumentsEvents2 specific functions
        //--------------------------------------------------------------------------------
        #region IVsTrackProjectDocumentsEvents2 interface funcions

        public int OnQueryAddFiles([InAttribute] IVsProject pProject, [InAttribute] int cFiles, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSQUERYADDFILEFLAGS[] rgFlags, [OutAttribute] VSQUERYADDFILERESULTS[] pSummaryResult, [OutAttribute] VSQUERYADDFILERESULTS[] rgResults)
        {
            _sccStatusTracker.EnableDirectoryWatching(false);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Implement this function to update the project scc glyphs when the items are added to the project.
        /// If a project doesn't call GetSccGlyphs as they should do (as solution folder do), this will update correctly the glyphs when the project is controled
        /// </summary>
        public int OnAfterAddFilesEx([InAttribute] int cProjects, [InAttribute] int cFiles, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSADDFILEFLAGS[] rgFlags)
        {
            _sccStatusTracker.EnableDirectoryWatching(true);

            HGLib.HGFileStatusInfo info;
            _sccStatusTracker.GetFileStatusInfo(rgpszMkDocuments[0], out info);
            if (info == null || info.status == HGLib.HGFileStatus.scsRemoved ||    // undelete file
                                info.status == HGLib.HGFileStatus.scsUncontrolled) // do not add files twice
            {
                // add only files wich are not ignored
                if (Configuration.Global._autoAddFiles)
                    _sccStatusTracker.AddWorkItem(new HGLib.TrackFilesAddedNotIgnored(rgpszMkDocuments));
            }
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
            _sccStatusTracker.EnableDirectoryWatching(false);
            return VSConstants.S_OK;
        }

        public int OnAfterRemoveFiles([InAttribute] int cProjects, [InAttribute] int cFiles, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSREMOVEFILEFLAGS[] rgFlags)
        {
            _sccStatusTracker.EnableDirectoryWatching(true);

            if (rgpProjects == null || rgpszMkDocuments == null)
                return VSConstants.E_POINTER;

            if (!File.Exists(rgpszMkDocuments[0])) // EnterFileRemoved only if the file was actually removed
                _sccStatusTracker.AddWorkItem(new HGLib.TrackFileRemoved(rgpszMkDocuments));

            return VSConstants.S_OK;
        }

        public int OnQueryRemoveDirectories([InAttribute] IVsProject pProject, [InAttribute] int cDirectories, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, [OutAttribute] VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, [OutAttribute] VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterRemoveDirectories([InAttribute] int cProjects, [InAttribute] int cDirectories, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            //StoreSolution();
            return VSConstants.S_OK;
        }

        public int OnQueryRenameFiles([InAttribute] IVsProject pProject, [InAttribute] int cFiles, [InAttribute] string[] rgszMkOldNames, [InAttribute] string[] rgszMkNewNames, [InAttribute] VSQUERYRENAMEFILEFLAGS[] rgFlags, [OutAttribute] VSQUERYRENAMEFILERESULTS[] pSummaryResult, [OutAttribute] VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            _sccStatusTracker.EnableDirectoryWatching(false);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Implement OnAfterRenameFiles event to rename a file in the source control store when it gets renamed in the project
        /// Also, rename the store if the project itself is renamed
        /// </summary>
        public int OnAfterRenameFiles([InAttribute] int cProjects, [InAttribute] int cFiles, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgszMkOldNames, [InAttribute] string[] rgszMkNewNames, [InAttribute] VSRENAMEFILEFLAGS[] rgFlags)
        {
            _sccStatusTracker.EnableDirectoryWatching(true);
            _sccStatusTracker.AddWorkItem(new HGLib.TrackFilesRenamed(rgszMkOldNames, rgszMkNewNames));
            return VSConstants.S_OK;
        }

        public int OnQueryRenameDirectories([InAttribute] IVsProject pProject, [InAttribute] int cDirs, [InAttribute] string[] rgszMkOldNames, [InAttribute] string[] rgszMkNewNames, [InAttribute] VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, [OutAttribute] VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, [OutAttribute] VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterRenameDirectories([InAttribute] int cProjects, [InAttribute] int cDirs, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgszMkOldNames, [InAttribute] string[] rgszMkNewNames, [InAttribute] VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterSccStatusChanged([InAttribute] int cProjects, [InAttribute] int cFiles, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] uint[] rgdwSccStatus)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion  // IVsTrackProjectDocumentsEvents2 interface funcions
    }
}
