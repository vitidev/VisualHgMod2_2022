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

        public static Process DiffDialog(string sccFile, string currentFile, string commandMask)
        {
            var root = HgProvider.FindRepositoryRoot(currentFile);

            if (String.IsNullOrEmpty(root))
            {
                return null;
            }

            // copy latest file revision from repo temp folder
            var versionedFile = Path.GetTempPath() + sccFile.Substring(sccFile.LastIndexOf("\\") + 1) + "(base)";

            // delete file if exists
            File.Delete(versionedFile);

            var cmd = "cat \"" + sccFile.Substring(root.Length + 1) + "\"  -o \"" + versionedFile + "\"";
            HgProvider.StartHg(root, cmd);

            // wait file exists on disk
            int counter = 0;
            while (!File.Exists(versionedFile) && counter < 10)
            {
                Thread.Sleep(100);
                ++counter;
            }

            // run diff tool
            if (!String.IsNullOrEmpty(commandMask))
            {
                cmd = PrepareDiffCommand(versionedFile, currentFile, commandMask);
                return HgProvider.Start(cmd, "", "");
            }
            
            commandMask = " \"$(Base)\" --fname \"$(BaseName)\" \"$(Mine)\" --fname \"$(MineName)\" ";
            cmd = PrepareDiffCommand(versionedFile, currentFile, commandMask);
         
            return HgProvider.StartKDiff(root, cmd);
        }

        private static string PrepareDiffCommand(string versionedFile, string currentFile, string commandMask)
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
            command = command.Replace("$(Base)", versionedFile);
            command = command.Replace("$(Mine)", currentFile);
            command = command.Replace("$(BaseName)", Path.GetFileName(versionedFile));
            command = command.Replace("$(MineName)", Path.GetFileName(currentFile));
            
            return command;
        }

        public static void ShowUpdateWindow(string directory)
        {
            Start("update", directory);
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

        public static void AnnotateDialog(string file)
        {
            var root = HgProvider.FindRepositoryRoot(file);
            
            if (!String.IsNullOrEmpty(root))
            {
                file = file.Substring(root.Length + 1);
                Start("annotate \"" + file + "\"", root);
            }
        }
    }
}
