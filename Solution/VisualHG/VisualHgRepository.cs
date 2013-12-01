using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HgLib;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    public class VisualHgRepository : HgRepository
    {
        private bool _solutionBuilding;
        private string[] lastAddition;

        public VisualHgFileSet SolutionFiles { get; private set; }

        public override HgFileInfo[] PendingFiles
        {
            get
            {
                return base.PendingFiles
                    .Where(x => SolutionFiles.Contains(x.FullName))
                    .ToArray();
            }
        }


        public VisualHgRepository()
        {
            SolutionFiles = new VisualHgFileSet();
        }

        public override void AddFiles(params string[] fileNames)
        {
            lastAddition = fileNames;
            SolutionFiles.Add(fileNames);

            base.AddFiles(fileNames);
        }

        public override void RemoveFiles(params string[] fileNames)
        {
            if (FilesMovedBetweenProjects(fileNames, lastAddition))
            {
                RenameFiles(fileNames, lastAddition);
            }

            base.RemoveFiles(fileNames);
        }

        public override void RenameFiles(string[] fileNames, string[] newFileNames)
        {
            SolutionFiles.Add(newFileNames);
         
            base.RenameFiles(fileNames, newFileNames);
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


        private static bool FilesMovedBetweenProjects(string[] removedFiles, string[] addedFiles)
        {
            if (removedFiles == null || addedFiles == null || removedFiles.Length != addedFiles.Length)
            {
                return false;
            }

            return removedFiles.SequenceEqual(addedFiles, new CrossProjectRenamesComparer());
        }


        private class CrossProjectRenamesComparer : IEqualityComparer<string>
        {
            private static readonly StringComparer Comparer = StringComparer.InvariantCultureIgnoreCase;

            public bool Equals(string x, string y)
            {
                return GetHashCode(x) == GetHashCode(y);
            }

            public int GetHashCode(string s)
            {
                return 
                    Comparer.GetHashCode(Path.GetFileName(s)) ^ 
                    Comparer.GetHashCode(HgPath.FindRepositoryRoot(s));
            }
        }
    }
}
