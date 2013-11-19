using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HgLib
{
    public static class TortoiseHg
    {
        public static string Version { get; private set; }


        static TortoiseHg()
        {
            Version = ProcessLauncher.RunTortoiseHg("version", "").FirstOrDefault();
        }


        public static void ShowCommitWindow(string directory)
        {
            Start("commit", directory);
        }

        public static void ShowWorkbenchWindow(string directory)
        {
            Start("workbench", directory);
        }

        public static void ShowStatusWindow(string directory)
        {
            Start("status", directory);
        }

        public static void ShowSynchronizeWindow(string directory)
        {
            Start("sync", directory);
        }

        public static void ShowUpdateWindow(string directory)
        {
            Start("update", directory);
        }

        public static void ShowAddWindow(string[] files)
        {
            StartForEachRoot("add ", files);
        }

        public static void ShowCommitWindow(string[] files)
        {
            StartForEachRoot("commit ", files);
        }

        public static void ShowRevertWindow(string[] files)
        {
            StartForEachRoot("revert", files);
        }

        public static void ShowHistoryWindow(string fileName)
        {
            var root = HgPath.FindRepositoryRoot(fileName);

            if (!String.IsNullOrEmpty(root))
            {
                fileName = fileName.Substring(root.Length + 1);

                Start(String.Format("history \"{0}\"", fileName), root);
            }
        }


        private static Process Start(string args, string workingDirectory)
        {
            while (!Directory.Exists(workingDirectory) && workingDirectory.Length > 0)
            {
                workingDirectory = workingDirectory.Substring(0, workingDirectory.LastIndexOf('\\'));
            }

            return ProcessLauncher.StartTortoiseHg(args, workingDirectory);
        }


        private static void StartForEachRoot(string command, string[] files)
        {
            var commandWithOptions = String.Concat("--nofork ", command);

            foreach (var group in files.GroupBy(x => HgPath.FindRepositoryRoot(x)))
            {
                if (String.IsNullOrEmpty(group.Key))
                {
                    continue;
                }

                Start(commandWithOptions, group.Key, group);
            }
        }

        private static void Start(string command, string root, IEnumerable<string> files)
        {
            var listFile = GetRandomTemporaryFileName();
            var listCommand = String.Format("{0} --listfile \"{1}\"", command, listFile);

            CreateListFile(listFile, files);

            Start(listCommand, root).WaitForExit();

            DeleteListFile(listFile);
        }

        private static void CreateListFile(string listFileName, IEnumerable<string> files)
        {
            using (var writer = File.CreateText(listFileName))
            {
                foreach (var fileName in files)
                {
                    writer.WriteLine(fileName);
                }
            }
        }

        private static void DeleteListFile(string groupListFile)
        {
            try
            {
                File.Delete(groupListFile);
            }
            catch { }
        }

        private static string GetRandomTemporaryFileName()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }
    }
}
