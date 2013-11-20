using System;
using System.IO;
using System.Linq;
using HgLib;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    public class VisualHgRepository : HgRepository
    {
        private bool _solutionBuilding;

        public VisualHgFileSet SolutionFiles { get; private set; }


        public VisualHgRepository()
        {
            SolutionFiles = new VisualHgFileSet();
        }


        public void SolutionBuildStarted()
        {
            _solutionBuilding = true;
        }

        public void SolutionBuildEnded()
        {
            _solutionBuilding = false;
        }

        public void UpdateSolution(IVsSolution solution)
        {
            foreach (var directory in GetProjectDirectories(solution))
            {
                UpdateRootStatus(directory);
            }
        }

        public void UpdateProject(IVsSccProject2 project)
        {
            UpdateRootStatus(VisualHgSolution.GetDirectoryName((IVsHierarchy)project));
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


        protected override void Update()
        {
            if (!_solutionBuilding)
            {
                base.Update();
            }
        }

        protected override bool FileChangeIsOfInterest(string fileName)
        {
            return SolutionFiles.Contains(fileName) && base.FileChangeIsOfInterest(fileName);
        }
    }
}
