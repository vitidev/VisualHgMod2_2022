using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace HgLib
{
    public static class TortoiseHg
    {
        static string tortoiseHgExecutablePath = null;
        
        public static string GetTortoiseHgExecutablePath()
        {
            if (String.IsNullOrEmpty(tortoiseHgExecutablePath))
            {
                var hgDir = Hg.GetTortoiseHgDirectory();

                var thg = Path.Combine(tortoiseHgExecutablePath, "thg.exe");
                var hgtk = Path.Combine(tortoiseHgExecutablePath, "hgtk.exe");

                tortoiseHgExecutablePath = File.Exists(thg) ? thg : File.Exists(hgtk) ? hgtk : null;
            }

            return tortoiseHgExecutablePath;
        }

        private static Process StartProcess(string executable, string workingDirectory, string arguments)
        {
            var process = new Process();
            
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = executable;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = workingDirectory;
            process.Start();
            
            return process;
        }

        private static Process InvokeCommand(string workingDirectory, string arguments)
        {
            if (workingDirectory != null && workingDirectory != string.Empty)
            {
                return StartProcess(GetTortoiseHgExecutablePath(), workingDirectory, arguments);
            }

            return null;
        }

        private static Process HgTKDialog(string directory, string dialog)
        {
            try
            {
                while (!Directory.Exists(directory) && directory.Length > 0)
                {
                    directory = directory.Substring(0, directory.LastIndexOf('\\'));
                }
                return InvokeCommand(directory, dialog);
            }
            catch (Exception ex)
            {
                // TortoiseHg HgTK commit dialog exception
                Trace.WriteLine("HgTK " + dialog + " dialog exception " + ex.Message);
            }

            return null;
        }

        static string PrepareDiffCommand(string versionedFile, string currentFile, string commandMask)
        {
            string programmFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programmFiles    = programmFilesX86;
            int index = programmFiles.IndexOf(" (x86)");
            if (index > 0)
                programmFiles = programmFiles.Substring(0, index);

            string command = commandMask;
            command = command.Replace("$(ProgramFiles (x86))", programmFilesX86);
            command = command.Replace("$(ProgramFiles)", programmFiles);
            command = command.Replace("$(Base)", versionedFile);
            command = command.Replace("$(Mine)", currentFile);
            command = command.Replace("$(BaseName)", Path.GetFileName(versionedFile));
            command = command.Replace("$(MineName)", Path.GetFileName(currentFile));
            return command;
        }

        public static Process DiffDialog(string sccFile, string file, string commandMask)
        {
            String root = HgLib.Hg.FindRepositoryRoot(file);
            if (root != String.Empty)
            {
                // copy latest file revision from repo temp folder
                string currentFile = file;
                string versionedFile = Path.GetTempPath() + sccFile.Substring(sccFile.LastIndexOf("\\") + 1) + "(base)";

                // delete file if exists
                File.Delete(versionedFile);

                string cmd = "cat \"" + sccFile.Substring(root.Length + 1) + "\"  -o \"" + versionedFile + "\"";
                StartProcess(Hg.GetHgExecutablePath(), root, cmd);

                // wait file exists on disk
                int counter = 0;
                while (!File.Exists(versionedFile) && counter < 10)
                { Thread.Sleep(100); ++counter; }

                // run diff tool
                if (commandMask != string.Empty)
                {
                    cmd = PrepareDiffCommand(versionedFile, currentFile, commandMask);
                    return StartProcess(cmd, "", "");
                }
                else
                {
                    commandMask = " \"$(Base)\" --fname \"$(BaseName)\" \"$(Mine)\" --fname \"$(MineName)\" ";
                    cmd = PrepareDiffCommand(versionedFile, currentFile, commandMask);
                    return StartProcess(Path.Combine(Hg.GetTortoiseHgDirectory(), "kdiff3.exe"), root, cmd);
                }
            }
            return null;
        }

        public static void CloneDialog(string directory)
        {
            HgTKDialog(directory, "clone");
        }

        public static void MergeDialog(string directory)
        {
            HgTKDialog(directory, "merge");
        }

        public static void UpdateDialog(string directory)
        {
            HgTKDialog(directory, "update");
        }

        public static void ConfigDialog(string directory)
        {
            HgTKDialog(directory, "userconfig");
        }

        public static void RecoveryDialog(string directory)
        {
            HgTKDialog(directory, "recovery");
        }

        public static void DataMineDialog(string directory)
        {
            HgTKDialog(directory, "datamine");
        }

        public static void HgTKSelectedFilesDialog(string[] files, string command)
        {
            string tmpFile = GetRandomTemporaryFileName();
            StreamWriter stream = new StreamWriter(tmpFile, false, Encoding.Default);

            string currentRoot = string.Empty;
            for (int n = 0; n < files.Length; ++n)
            {
                string root = HgLib.Hg.FindRepositoryRoot(files[n]);
                if (root == string.Empty)
                    continue;

                if (currentRoot == string.Empty)
                {
                    currentRoot = root;
                }
                else if (string.Compare(currentRoot, root, true) != 0)
                {
                    stream.Close();
                    Process process = HgTKDialog(root, command + " --listfile \"" + tmpFile + "\"");
                    process.WaitForExit();

                    tmpFile = GetRandomTemporaryFileName();
                    stream = new StreamWriter(tmpFile, false, Encoding.Default);
                }

                stream.WriteLine(files[n]);
            }

            stream.Close();
            if (currentRoot != string.Empty)
            {
                Process process2 = HgTKDialog(currentRoot, command + " --listfile \"" + tmpFile + "\"");
                process2.WaitForExit();
            }
        }

        public static void AnnotateDialog(string file)
        {
            String root = HgLib.Hg.FindRepositoryRoot(file);
            if (root != String.Empty)
            {
                file = file.Substring(root.Length + 1);
                HgTKDialog(root, "annotate \"" + file + "\"");
            }
        }

        private static string GetRandomTemporaryFileName()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }
    }
}
