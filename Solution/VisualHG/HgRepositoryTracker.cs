using System;
using System.IO;
using System.Linq;
using HgLib;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    public class HgRepositoryTracker : HgRepository
    {
        public void UpdateProject(IVsSccProject2 project)
        {
            UpdateRootDirectory(ProjectHelper.GetDirectoryName((IVsHierarchy)project));
        }
        
        public void UpdateProjects(IVsSolution solution)
        {
            foreach (var directory in GetProjectDirectories(solution))
            {
                UpdateRootDirectory(directory);
            }
        }

        private void UpdateRootDirectory(string directory)
        {
            Enqueue(new AddRootDirectoryHgCommand(directory));
        }

        private static string[] GetProjectDirectories(IVsSolution solution)
        {
            return GetProjectFiles(solution)
                .Where(x => !String.IsNullOrEmpty(x))
                .Select(x => Path.GetDirectoryName(x) + '\\')
                .ToArray();
        }

        private static string[] GetProjectFiles(IVsSolution solution)
        {
            uint numberOfProjects;
            solution.GetProjectFilesInSolution(0, 0, null, out numberOfProjects);

            var projectFiles = new string[numberOfProjects];
            solution.GetProjectFilesInSolution(0, numberOfProjects, projectFiles, out numberOfProjects);
            
            return projectFiles;
        }
    }
}
