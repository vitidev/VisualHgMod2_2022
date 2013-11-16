using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace HgLib
{
    public static class Hg
    {
        public static string GetCurrentBranchName(string root)
        {
            return RunHg(root, "branch").FirstOrDefault() ?? "";
        }


        public static Dictionary<string, char> GetRootStatus(string directory)
        {
            var status = new Dictionary<string, char>();

            if (!String.IsNullOrEmpty(directory))
            {
                var nameHistory = new Dictionary<string, string>();

                var output = RunHg(directory, "status -m -a -r -d -c -C ");

                UpdateStatus(directory, output, status, nameHistory);
            }

            return status;
        }

        public static Dictionary<string, char> GetFileStatus(string[] fileNames)
        {
            Dictionary<string, string> nameHistory;
            return GetFileStatus(fileNames, out nameHistory);
        }

        private static Dictionary<string, char> GetFileStatus(string[] fileNames, out Dictionary<string, string> nameHistory)
        {
            var status = new Dictionary<string, char>();
            nameHistory = new Dictionary<string, string>();
            var commandLines = new Dictionary<string, string>();

            try
            {
                if (fileNames.Length > 0)
                {
                    foreach (var fileName in fileNames)
                    {
                        var root = HgProvider.FindRepositoryRoot(fileName);

                        var commandLine = "";
                        commandLines.TryGetValue(root, out commandLine);
                        commandLine += " \"" + fileName.Substring(root.Length + 1) + "\" ";

                        if (commandLine.Length >= 2000)
                        {
                            var output = RunHg(root, "status -A " + commandLine);
                            UpdateStatus(root, output, status, nameHistory);

                            commandLine = "";
                        }

                        commandLines[root] = commandLine;
                    }

                    foreach (var directoryCommandLine in commandLines)
                    {
                        var root = directoryCommandLine.Key;
                        var commandLine = directoryCommandLine.Value;

                        if (commandLine.Length > 0)
                        {
                            var output = RunHg(root, "status -A " + commandLine);
                            UpdateStatus(root, output, status, nameHistory);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return status;
        }


        public static Dictionary<string, char> AddFiles(string[] fileNames)
        {
            var filesToAdd = GetFilesToAdd(fileNames);

            if (filesToAdd.Length == 0)
            {
                return null;
            }

            return GetStatus("add", filesToAdd);
        }

        private static string[] GetFilesToAdd(string[] fileNames)
        {
            var filesToAdd = new List<string>();

            var status = GetFileStatus(fileNames);

            foreach (var fileStatus in status)
            {
                if (fileStatus.Value == '?' || fileStatus.Value == 'R')
                {
                    filesToAdd.Add(fileStatus.Key);
                }
            }
            
            return filesToAdd.ToArray();
        }

        public static List<string> Update(string root)
        {
            return RunHg(root, "update -C");
        }


        public static bool RenameFiles(string[] oldFileNames, string[] newFileNames)
        {
            try
            {
                for (int i = 0; i < oldFileNames.Length; ++i)
                {
                    var workingDirectory = Path.GetDirectoryName(oldFileNames[i]);
                    var rootDirectory = HgProvider.FindRepositoryRoot(workingDirectory);

                    var oldName = oldFileNames[i].Substring(rootDirectory.Length + 1);
                    var newName = newFileNames[i].Substring(rootDirectory.Length + 1);
                    
                    RunHg(rootDirectory, String.Format("rename -A \"{0}}\" \"{1}\"", oldName, newName));
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static Dictionary<string, char> RemoveFiles(string[] fileNames)
        {
            return GetStatus("remove", fileNames);
        }


        public static string GetRenamedFileOriginalName(string newFileName)
        {
            var originalFileName = "";
            
            Dictionary<string, string> nameHistory;
            var status = GetFileStatus(new[] { newFileName }, out nameHistory);
            
            if (status != null)
            {
                nameHistory.TryGetValue(newFileName.ToLower(), out originalFileName);
            }

            return originalFileName;
        }


        private static Dictionary<string, char> GetStatus(string command, string[] fileNames)
        {
            try
            {
                RunHg(command, fileNames, false);
                return GetFileStatus(fileNames);
            }
            catch
            {
                return null;
            }
        }

        private static bool UpdateStatus(string root, List<string> output, Dictionary<string, char> status, Dictionary<string, string> nameHistory)
        {
            var copyRenamedFiles = new Dictionary<string, string>();

            var prevStatus = ' ';
            var prevFile = "";

            foreach (var line in output)
            {
                var file = Path.Combine(root, line.Substring(2));
                var currentStatus = line[0];

                if (!File.Exists(file))
                {
                    continue;
                }

                if (currentStatus == ' ' && prevStatus == 'A')
                {
                    copyRenamedFiles[file] = prevFile;
                    nameHistory[prevFile.ToLower()] = file;
                    file = prevFile;
                }

                status[file] = currentStatus;

                prevFile = file;
                prevStatus = currentStatus;
            }

            foreach (var entry in copyRenamedFiles)
            {
                char orgFileStatus;

                if (!status.TryGetValue(entry.Key, out orgFileStatus))
                {
                    var fileStatusOriginalFile = GetFileStatus(new[] { entry.Key });

                    if (fileStatusOriginalFile != null)
                    {
                        fileStatusOriginalFile.TryGetValue(entry.Key, out orgFileStatus);
                    }
                }

                if (orgFileStatus == 'R')
                {
                    status[entry.Value] = 'N';
                }
                else
                {
                    status[entry.Value] = 'P';
                }
            }

            return true;
        }


        private static List<string> RunHg(string workingDirectory, string arguments)
        {
            var process = HgProvider.StartHg(arguments, workingDirectory);

            return ReadStandardOutputFrom(process);
        }

        private static void RunHg(string command, string[] paths, bool includeDirectories)
        {
            var workingDirectory = Path.GetDirectoryName(paths[0]);
            var rootDirectory = HgProvider.FindRepositoryRoot(workingDirectory);
            var args = command;

            var counter = 0;

            foreach (var path in paths)
            {
                counter++;

                if (!includeDirectories && IsDirectory(path))
                {
                    continue;
                }

                args += " \"" + path.Substring(rootDirectory.Length + 1) + "\" ";

                if (counter > 20 || args.Length > 1024)
                {
                    RunHg(rootDirectory, args);
                    args = command;
                }
            }

            RunHg(rootDirectory, args);
        }

        public static bool IsDirectory(string path)
        {
            if (File.Exists(path) || Directory.Exists(path))
            {
                return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }

            return false;
        }

        
        private static List<string> ReadStandardOutputFrom(Process process)
        {
            var str = "";
            var outputLines = new List<string>();

            while (!process.HasExited)
            {
                while ((str = process.StandardOutput.ReadLine()) != null)
                {
                    outputLines.Add(str);
                }

                Thread.Sleep(0);
            }

            while ((str = process.StandardOutput.ReadLine()) != null)
            {
                outputLines.Add(str);
            }

            return outputLines;
        }
    }
}