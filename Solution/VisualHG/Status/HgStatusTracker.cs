using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;


namespace VisualHg
{

    // ---------------------------------------------------------------------------
    // 
    // Solution file status cache.
    //
    // To keep the files up to date we react to SccProviderSrvice requests and also
    // handles directory watcher events.
    //
    // ---------------------------------------------------------------------------
    public class HgStatusTracker : HgLib.HgRepository
    {
        /// <summary>
        /// Called by SccProviderSrvice when a scc-capable project is opened
        /// </summary>
        /// <param name="project">The loaded project</param>
        /// <param name="added">The project was added after opening</param>
        internal void UpdateProject(IVsSccProject2 project)
        {
            if (project != null)
            {
                string projectDirectory = SccProjectData.ProjectDirectory((IVsHierarchy)project);
                string projectName = SccProjectData.ProjectName((IVsHierarchy)project);

                Enqueue(new HgLib.UpdateRootDirectoryAdded(projectDirectory));
            }
        }
        
        public void UpdateProjects(IVsSolution sol)
        {
            uint numberOfProjects;
            sol.GetProjectFilesInSolution(0, 0, null, out numberOfProjects);
            string[] projectFiles = new string[numberOfProjects];
            sol.GetProjectFilesInSolution(0, numberOfProjects, projectFiles, out numberOfProjects);

            foreach (string projectFile in projectFiles)
            {
                if (!String.IsNullOrEmpty(projectFile))
                {
                    string projectDirectory = projectFile.Substring(0, projectFile.LastIndexOf("\\") + 1);
                    Enqueue(new HgLib.UpdateRootDirectoryAdded(projectDirectory));
                }
            }
        }
    }
}
