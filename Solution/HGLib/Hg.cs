using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Win32;

namespace HgLib
{
    public static class Hg
    {
        private static string hgDir = null;
        private static string hgexe = null;

        public static string TemporaryFile
        {
            get
            {
                return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }
        }


        public static string GetTortoiseHgDirectory()
        {
            if (String.IsNullOrEmpty(hgDir))
            {
                var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\TortoiseHg");

                if (key == null)
                {
                    key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\TortoiseHg");
                }

                if (key != null)
                {
                    hgDir = (string)key.GetValue(null);
                }
            }

            return hgDir;
        }

        public static string GetHgExecutablePath()
        {
            if (String.IsNullOrEmpty(hgexe))
            {
                var dir = GetTortoiseHgDirectory();

                hgexe = "Hg.exe";

                if (!String.IsNullOrEmpty(dir))
                {
                    hgexe = Path.Combine(dir, hgexe);
                }
            }

            return hgexe;
        }

        private static Process InvokeCommand(string workingDirectory, string arguments)
        {
            Trace.WriteLine("InvokeCommand : " + arguments);

            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = GetHgExecutablePath();
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = workingDirectory;
            process.Start();

            return process;
        }

        public static bool InvokeCommand(string workingDirectory, string arguments, out List<string> output)
        {
            var process = InvokeCommand(workingDirectory, arguments);

            output = ReadStandardOutputFrom(process);

            return output.Count > 0;
        }

        private static void InvokeCommand(string command, string[] paths, bool directoriesAllowed)
        {
            List<string> list;

            var workingDirectory = Path.GetDirectoryName(paths[0]);
            var rootDirectory = FindRepositoryRoot(workingDirectory);
            var commandString = command;

            var counter = 0;

            foreach (var path in paths)
            {
                counter++;

                if (!directoriesAllowed && IsDirectory(path))
                {
                    continue;
                }

                commandString += " \"" + path.Substring(rootDirectory.Length + 1) + "\" ";

                if (counter > 20 || commandString.Length > 1024)
                {
                    InvokeCommand(rootDirectory, commandString, out list);
                    commandString = command;
                }
            }

            InvokeCommand(rootDirectory, commandString, out list);
        }


        private static bool InvokeCommandGetStatus(string cmd, string[] fileList, out Dictionary<string, char> fileStatusDictionary)
        {
            fileStatusDictionary = null;

            try
            {
                InvokeCommand(cmd, fileList, false);

                if (!QueryFileStatus(fileList, out fileStatusDictionary))
                {
                    fileStatusDictionary = null;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("cmd- " + ex.Message);
                fileStatusDictionary = null;
            }

            return fileStatusDictionary != null;
        }

        public static string FindRepositoryRoot(string path)
        {
            while (path.Length > 0)
            {
                if (Directory.Exists(Path.Combine(path, ".hg")))
                {
                    break;
                }

                path = GetParentDirectory(path);
            }

            return path;
        }

        public static string GetCurrentBranchName(string rootDirectory)
        {
            var branchName = "";

            List<string> output;

            InvokeCommand(rootDirectory, "branch", out output);

            if (output.Count > 0)
            {
                branchName = output[0];
            }

            return branchName;
        }


        public static bool QueryRootStatus(string rootDirectory, out Dictionary<string, char> fileStatusDictionary)
        {
            Trace.WriteLine("Start QueryRootStatus");

            fileStatusDictionary = null;

            if (!String.IsNullOrEmpty(rootDirectory))
            {
                var renamedToOrgFileDictionary = new Dictionary<string, string>();

                List<string> output;
                InvokeCommand(rootDirectory, "status -m -a -r -d -c -C ", out output);

                fileStatusDictionary = new Dictionary<string, char>();
                UpdateStatusDictionary(output, rootDirectory, fileStatusDictionary, renamedToOrgFileDictionary);
            }

            return fileStatusDictionary != null;
        }

        public static bool QueryFileStatus(string fileName, out Dictionary<string, char> fileStatusDictionary, out Dictionary<string, string> renamedToOrgFileDictionary)
        {
            return QueryFileStatus(new string[] { fileName }, out fileStatusDictionary, out renamedToOrgFileDictionary);
        }

        public static bool QueryFileStatus(string[] fileList, out Dictionary<string, char> fileStatusDictionary)
        {
            Dictionary<string, string> renamedToOrgFileDictionary;
            return QueryFileStatus(fileList, out fileStatusDictionary, out renamedToOrgFileDictionary);
        }

        public static bool QueryFileStatus(string fileName, out Dictionary<string, char> fileStatusDictionary)
        {
            Dictionary<string, string> renamedToOrgFileDictionary;
            return QueryFileStatus(fileName, out fileStatusDictionary, out renamedToOrgFileDictionary);
        }

        public static bool QueryFileStatus(string[] fileList, out Dictionary<string, char> fileStatusDictionary, out Dictionary<string, string> renamedToOrgFileDictionary)
        {
            fileStatusDictionary = new Dictionary<string, char>();
            renamedToOrgFileDictionary = new Dictionary<string, string>();
            var commandLines = new Dictionary<string, string>();

            try
            {
                if (fileList.Length > 0)
                {
                    foreach (var fileName in fileList)
                    {
                        var rootDirectory = FindRepositoryRoot(fileName);

                        var commandLine = "";
                        commandLines.TryGetValue(rootDirectory, out commandLine);
                        commandLine += " \"" + fileName.Substring(rootDirectory.Length + 1) + "\" ";

                        if (commandLine.Length >= 2000)
                        {
                            List<string> output;
                            InvokeCommand(rootDirectory, "status -A " + commandLine, out output);
                            UpdateStatusDictionary(output, rootDirectory, fileStatusDictionary, renamedToOrgFileDictionary);

                            commandLine = "";
                        }

                        commandLines[rootDirectory] = commandLine;
                    }

                    foreach (var directoryCommandLine in commandLines)
                    {
                        var rootDirectory = directoryCommandLine.Key;
                        var commandLine = directoryCommandLine.Value;
                        
                        if (commandLine.Length > 0)
                        {
                            List<string> output;
                            InvokeCommand(rootDirectory, "status -A " + commandLine, out output);
                            UpdateStatusDictionary(output, rootDirectory, fileStatusDictionary, renamedToOrgFileDictionary);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("HgProcess.QueryFileStatus: " + ex.Message);
                return false;
            }

            return (fileStatusDictionary != null);
        }

        private static bool UpdateStatusDictionary(List<string> lines, string rootDirectory, Dictionary<string, char> fileStatusDictionary, Dictionary<string, string> renamedToOrgFileDictionary)
        {
            var copyRenamedFiles = new Dictionary<string, string>();

            var prevStatus = ' ';
            var prevFile = "";

            foreach (var str in lines)
            {
                var file = Path.Combine(rootDirectory, str.Substring(2));
                var status = str[0];

                if (!File.Exists(file))
                {
                    continue;
                }

                if (status == ' ' && prevStatus == 'A')
                {
                    copyRenamedFiles[file] = prevFile;
                    renamedToOrgFileDictionary[prevFile.ToLower()] = file;
                    file = prevFile;
                }

                fileStatusDictionary[file] = status;

                prevFile = file;
                prevStatus = status;
            }

            foreach (var entry in copyRenamedFiles)
            {
                char orgFileStatus;

                if (!fileStatusDictionary.TryGetValue(entry.Key, out orgFileStatus))
                {
                    Dictionary<string, char> fileStatusOriginalFile;

                    if (QueryFileStatus(entry.Key, out fileStatusOriginalFile))
                    {
                        fileStatusOriginalFile.TryGetValue(entry.Key, out orgFileStatus);
                    }
                }

                if (orgFileStatus == 'R')
                {
                    fileStatusDictionary[entry.Value] = 'N';
                }
                else
                {
                    fileStatusDictionary[entry.Value] = 'P';
                }
            }

            return true;
        }


        public static bool AddFiles(string[] fileList, out Dictionary<string, char> fileStatusDictionary)
        {
            return InvokeCommandGetStatus("add", fileList, out fileStatusDictionary);
        }

        public static bool AddFilesNotIgnored(string[] fileList, out Dictionary<string, char> fileStatusDictionary)
        {
            var addFilesList = new List<string>();

            Dictionary<string, char> statusDictionary;
            QueryFileStatus(fileList, out statusDictionary);
            
            foreach (var item in statusDictionary)
            {
                if (item.Value == '?' || item.Value == 'R')
                {
                    addFilesList.Add(item.Key);
                }
            }

            if (addFilesList.Count > 0)
            {
                return InvokeCommandGetStatus("add", addFilesList.ToArray(), out fileStatusDictionary);
            }

            fileStatusDictionary = null;
            return false;
        }


        public static bool EnterFileRenamed(string[] orgFileName, string[] newFileName)
        {
            try
            {
                for (int i = 0; i < orgFileName.Length; ++i)
                {
                    var workingDirectory = orgFileName[i].Substring(0, orgFileName[i].LastIndexOf('\\'));
                    var rootDirectory = FindRepositoryRoot(workingDirectory);

                    var ofile = orgFileName[i].Substring(rootDirectory.Length + 1);
                    var nfile = newFileName[i].Substring(rootDirectory.Length + 1);
                    List<string> output;
                    InvokeCommand(rootDirectory, "rename  -A \"" + ofile + "\" \"" + nfile + "\"", out output);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Hg.EnterFileRenamed exception- " + ex.Message);
                return false;
            }

            return true;
        }

        public static bool EnterFileRemoved(string[] fileList, out Dictionary<string, char> fileStatusDictionary)
        {
            return InvokeCommandGetStatus("remove", fileList, out fileStatusDictionary);
        }


        public static bool Revert(string[] fileList, out Dictionary<string, char> fileStatusDictionary)
        {
            return InvokeCommandGetStatus("revert", fileList, out fileStatusDictionary);
        }

        public static string GetOriginalOfRenamedFile(string newFileName)
        {
            var originalFileName = "";
            Dictionary<string, char> fileStatusDictionary;
            Dictionary<string, string> renamedToOrgFileDictionary;
            if (QueryFileStatus(newFileName, out fileStatusDictionary, out renamedToOrgFileDictionary))
            {
                renamedToOrgFileDictionary.TryGetValue(newFileName.ToLower(), out originalFileName);
            }
            return originalFileName;
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

        private static bool IsDirectory(string file)
        {
            if (File.Exists(file) || Directory.Exists(file))
            {
                return File.GetAttributes(file).HasFlag(FileAttributes.Directory);
            }

            return false;
        }

        private static string GetParentDirectory(string path)
        {
            DirectoryInfo parent;

            try
            {
                parent = Directory.GetParent(path);
            }
            catch
            {
                parent = null;
            }

            return parent != null ? parent.ToString() : "";
        }
    }
}