using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using HgLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    public partial class SccProviderService
    {
        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            if (!Active && Configuration.Global.AutoActivatePlugin)
            {
                var root = _sccProvider.GetRootDirectory();
                
                if (!String.IsNullOrEmpty(root))
                {
                    var rscp = _sccProvider.GetService(typeof(IVsRegisterScciProvider)) as IVsRegisterScciProvider;
                    
                    rscp.RegisterSourceControlProvider(Guids.guidSccProvider);
                }
            }

            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            _sccStatusTracker.ClearCache();
            _sccProvider._LastSeenProjectDir = "";
            
            UpdatePendingWindowState();

            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            _sccProvider._LastSeenProjectDir = ProjectHelper.GetDirectoryName(pRealHierarchy);

            var project = pRealHierarchy as IVsSccProject2;

            if (project != null)
            {
                _sccStatusTracker.UpdateProject(project);
            }

            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            var project = pHierarchy as IVsSccProject2;

            var files = SccProvider.GetProjectFiles(project).ToArray();

            if (files.Length > 0)
            {
                _sccStatusTracker.AddFilesToProjectCache(files);

                if (Configuration.Global.AutoAddFiles)
                {
                    _sccStatusTracker.Enqueue(new AddFilesHgCommand(files));
                }
                else
                {
                    _sccStatusTracker.Enqueue(new UpdateFileStatusHgCommand(files));
                }
            }
            
            if (project != null)
            {
                _sccStatusTracker.UpdateProject(project);
            }

            _sccProvider._LastSeenProjectDir = ProjectHelper.GetDirectoryName(pHierarchy);


            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            if (_sccStatusTracker.FileProjectMapCacheCount > 0)
            { 
                var project = pHierarchy as IVsSccProject2;
                var files = SccProvider.GetProjectFiles(project);
                
                _sccStatusTracker.RemoveFilesFromProjectCache(files);
            }
                        
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            _sccStatusTracker.ClearProjectCache();

            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            if (_sccStatusTracker.FileProjectMapCacheCount > 0)
            {
                var project = pRealHierarchy as IVsSccProject2;
                var files = SccProvider.GetProjectFiles(project);
                
                _sccStatusTracker.RemoveFilesFromProjectCache(files);
            }

            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterMergeSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }


        public int OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags, [Out] VSQUERYADDFILERESULTS[] pSummaryResult, [Out] VSQUERYADDFILERESULTS[] rgResults)
        {
            _sccStatusTracker.FileSystemWatch = false;
            
            return VSConstants.S_OK;
        }

        public int OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            _sccStatusTracker.FileSystemWatch = true;

            if (Configuration.Global.AutoAddFiles)
            {
                HgFileInfo info;
                _sccStatusTracker.GetFileInfo(rgpszMkDocuments[0], out info);

                if (info == null || info.Status == HgFileStatus.Removed || info.Status == HgFileStatus.Uncontrolled)
                {
                    _sccStatusTracker.Enqueue(new AddFilesHgCommand(rgpszMkDocuments));
                }
            }

            return VSConstants.S_OK;
        }

        public int OnQueryAddDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags, [Out] VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, [Out] VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, [Out] VSQUERYREMOVEFILERESULTS[] pSummaryResult, [Out] VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            _sccStatusTracker.FileSystemWatch = false;
            return VSConstants.S_OK;
        }

        public int OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            _sccStatusTracker.FileSystemWatch = true;

            if (rgpProjects == null || rgpszMkDocuments == null)
            {
                return VSConstants.E_POINTER;
            }

            if (!File.Exists(rgpszMkDocuments[0]))
            {
                _sccStatusTracker.Enqueue(new RemoveFilesHgCommand(rgpszMkDocuments));
            }

            return VSConstants.S_OK;
        }

        public int OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, [Out] VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, [Out] VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, [Out] VSQUERYRENAMEFILERESULTS[] pSummaryResult, [Out] VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            _sccStatusTracker.FileSystemWatch = false;
            
            return VSConstants.S_OK;
        }

        public int OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            _sccStatusTracker.FileSystemWatch = true;
            _sccStatusTracker.Enqueue(new RenameFilesHgCommand(rgszMkOldNames, rgszMkNewNames));
            
            return VSConstants.S_OK;
        }

        public int OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, [Out] VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, [Out] VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus)
        {
            return VSConstants.E_NOTIMPL;
        }
    }
}