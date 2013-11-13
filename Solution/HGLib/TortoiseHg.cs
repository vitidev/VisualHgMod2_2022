using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace HgLib
{
    public static class TortoiseHg
    {
        public static Process Start(string dialog, string workingDirectory)
        {
            try
            {
                while (!Directory.Exists(workingDirectory) && workingDirectory.Length > 0)
                {
                    workingDirectory = workingDirectory.Substring(0, workingDirectory.LastIndexOf('\\'));
                }

                if (!String.IsNullOrEmpty(workingDirectory))
                {
                    return HgProvider.StartTortoiseHg(dialog, workingDirectory);
                }
            }
            catch { }
            
            return null;
        }


        public static Process DiffDialog(string parent, string current, string customDiffTool)
        {
            var workingDirectory = HgProvider.FindRepositoryRoot(current);

            if (String.IsNullOrEmpty(workingDirectory))
            {
                return null;
            }

            var temp = CreateParentRevisionTempFile(parent, workingDirectory);

            if (!String.IsNullOrEmpty(customDiffTool))
            {
                return StartCustomDiffTool(current, customDiffTool, temp);
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

        private static Process StartCustomDiffTool(string current, string customDiffTool, string temp)
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


        public static void ShowSelectedFilesWindow(string[] files, string command)
        {
            var tmpFile = GetRandomTemporaryFileName();
            var stream = new StreamWriter(tmpFile, false, Encoding.Default);

            var currentRoot = "";
            
            for (var n = 0; n < files.Length; ++n)
            {
                var root = HgProvider.FindRepositoryRoot(files[n]);

                if (String.IsNullOrEmpty(root))
                { 
                    continue;
                }

                if (currentRoot == "")
                {
                    currentRoot = root;
                }
                else if (String.Compare(currentRoot, root, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    stream.Close();
                    
                    Start(command + " --listfile \"" + tmpFile + "\"", root).WaitForExit();

                    tmpFile = GetRandomTemporaryFileName();
                    stream = new StreamWriter(tmpFile, false, Encoding.Default);
                }

                stream.WriteLine(files[n]);
            }

            stream.Close();

            if (currentRoot != "")
            {
                Start(command + " --listfile \"" + tmpFile + "\"", currentRoot).WaitForExit();
            }
        }

        private static string GetRandomTemporaryFileName()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }
    }
}
