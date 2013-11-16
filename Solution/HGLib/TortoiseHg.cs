using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace HgLib
{
    public static class TortoiseHg
    {
        public static Process Start(string args, string workingDirectory)
        {
            try
            {
                while (!Directory.Exists(workingDirectory) && workingDirectory.Length > 0)
                {
                    workingDirectory = workingDirectory.Substring(0, workingDirectory.LastIndexOf('\\'));
                }

                if (!String.IsNullOrEmpty(workingDirectory))
                {
                    return HgProvider.StartTortoiseHg(args, workingDirectory);
                }
            }
            catch { }
            
            return null;
        }


        public static void StartForEachRoot(string command, string[] files)
        {
            foreach (var group in files.GroupBy(x => HgProvider.FindRepositoryRoot(x)))
            {
                if (String.IsNullOrEmpty(group.Key))
                {
                    continue;
                }

                Start(command, group.Key, group);
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


        public static Process StartDiff(string parent, string current, string customDiffTool)
        {
            var workingDirectory = HgProvider.FindRepositoryRoot(current);

            if (String.IsNullOrEmpty(workingDirectory))
            {
                return null;
            }

            var temp = CreateParentRevisionTempFile(parent, workingDirectory);

            if (!String.IsNullOrEmpty(customDiffTool))
            {
                return StartCustomDiff(current, customDiffTool, temp);
            }

            return StartKDiff(current, workingDirectory, temp);
        }

        private static string CreateParentRevisionTempFile(string fileName, string root)
        {
            var tempFileName = GetTempFileName(fileName);

            File.Delete(tempFileName);

            var cmd = String.Format("cat \"{0}\"  -o \"{1}\"", fileName.Replace(root + "\\", ""), tempFileName);
            HgProvider.StartHg(cmd, root).WaitForExit();

            Debug.Assert(File.Exists(tempFileName));

            return tempFileName;
        }

        private static string GetTempFileName(string fileName)
        {
            return Path.Combine(Path.GetTempPath(), Path.GetFileName(fileName) + " (base)");
        }

        private static Process StartKDiff(string current, string root, string temp)
        {
            var cmd = PrepareDiffCommand(temp, current, " \"$(Base)\" --fname \"$(BaseName)\" \"$(Mine)\" --fname \"$(MineName)\" ");

            return HgProvider.StartKDiff(cmd, root);
        }

        private static Process StartCustomDiff(string current, string customDiffTool, string temp)
        {
            var cmd = PrepareDiffCommand(temp, current, customDiffTool);
            
            return HgProvider.Start(cmd, "", "");
        }

        private static string PrepareDiffCommand(string parent, string current, string commandMask)
        {
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFiles    = programFilesX86;
            var index = programFiles.IndexOf(" (x86)");

            if (index > 0)
            {
                programFiles = programFiles.Substring(0, index);
            }
                
            var command = commandMask;
            
            command = command.Replace("$(ProgramFiles (x86))", programFilesX86);
            command = command.Replace("$(ProgramFiles)", programFiles);
            command = command.Replace("$(Base)", parent);
            command = command.Replace("$(Mine)", current);
            command = command.Replace("$(BaseName)", Path.GetFileName(parent));
            command = command.Replace("$(MineName)", Path.GetFileName(current));
            
            return command;
        }
    }
}
