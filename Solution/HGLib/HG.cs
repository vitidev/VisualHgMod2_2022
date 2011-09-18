using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;

namespace HGLib
{
    // ------------------------------------------------------------------------
    // HG interface class - here we cover HG.exe commands in sync and async manner.
    // There are methods for file status queries, rename, add or remove files from
    // repo and update commands included. All these methods are defined as static to
    // become easy to use.
    // ------------------------------------------------------------------------
    public static class HG
    {
        #region invoke HG commands

      // ------------------------------------------------------------------------
      // tortois hg install directory
      // ------------------------------------------------------------------------
      static string hgDir = null;
      public static string GetTortoiseHGDirectory()
      {
        if (hgDir == null || hgDir == string.Empty)
        {
          RegistryKey regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\TortoiseHg");
          if (regKey == null)
            regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\TortoiseHg");
            
          if (regKey != null)
          {
            hgDir = (string)regKey.GetValue(null);

            if (hgDir != null && hgDir != string.Empty)
            {
              if (!hgDir.EndsWith("\\"))
                hgDir += "\\";
            }
          }
        }
        return hgDir;
      }

      // ------------------------------------------------------------------------
      // get hg.exe with full path
      // ------------------------------------------------------------------------
      static string hgexe = null;
      public static string GetHGFileName()
      {
        if (hgexe == null || hgexe == string.Empty)
        {
          hgexe = GetTortoiseHGDirectory();

          if (hgexe != null && hgexe != string.Empty)
          {
            if (hgexe.EndsWith("\\"))
              hgexe += "HG.exe";
            else
              hgexe += "\\HG.exe";
          }
          else
          {
            hgexe = "HG.exe";
          }
        }

        return hgexe;
      }
      
      // ------------------------------------------------------------------------
        // invoke HG exe commands
        // ------------------------------------------------------------------------
        static Process InvokeCommand(string workingDirectory, string arguments)
        {
            Trace.WriteLine("InvokeCommand : " + arguments);

            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = GetHGFileName();
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = workingDirectory;
            process.Start();
            return process;
        }

        // ------------------------------------------------------------------------
        // invoke HG exe commands and read results
        // ------------------------------------------------------------------------
        public static bool InvokeCommand(string workingDirectory, string arguments, out List<string> resultList)
        {
            resultList = new List<string>();
            
            Process process = InvokeCommand(workingDirectory, arguments);

            string str = "";
            while (!process.HasExited)
            {
                while ((str = process.StandardOutput.ReadLine()) != null)
                {
                    resultList.Add(str);
                }
                Thread.Sleep(0);
            }

            while ((str = process.StandardOutput.ReadLine()) != null)
            {
                resultList.Add(str);
            }

            return (resultList.Count > 0);
        }

        // ------------------------------------------------------------------------
        // build the argument sting from the given files and call invoke the hg command
        // ------------------------------------------------------------------------
        static void InvokeCommand(string command, string[] fileList, bool directoriesAllowed)
        {
            string workingDirectory = fileList[0].Substring(0, fileList[0].LastIndexOf('\\'));
            string rootDirectory = HG.FindRootDirectory(workingDirectory);
            List<string> list;
            string commandString = command;
            int counter = 0;
            foreach (var file in fileList)
            {
                ++counter; 

                if (!directoriesAllowed && file.EndsWith("\\"))
                    continue;

                commandString += " \"" + file.Substring(rootDirectory.Length + 1) + "\" ";

                if (counter > 20 || commandString.Length > 1024)
                { 
                    HG.InvokeCommand(rootDirectory, commandString, out list);
                    commandString = command;
                }
            }

            
            HG.InvokeCommand(rootDirectory, commandString, out list);
        }

        // ------------------------------------------------------------------------
        // invoke the given command and get the new status of dependend files
        // to the dictionary
        // ------------------------------------------------------------------------
        static bool InvokeCommandGetStatus(string cmd, string[] fileList, out Dictionary<string, char> fileStatusDictionary)
        {
            fileStatusDictionary = null;
            try
            {
                HG.InvokeCommand(cmd, fileList, false);

                if (!QueryFileStatus(fileList, out fileStatusDictionary))
                    fileStatusDictionary = null;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("cmd- " + ex.Message);
                return false;

            }
            return (fileStatusDictionary != null);
        }

        #endregion invoke commands

        // ------------------------------------------------------------------------
        // create temporary filename in system temp folder
        // ------------------------------------------------------------------------
        static public string TemporaryFile
        {
            get
            {
                string TemporaryFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                return TemporaryFile;
            }
        }

        // ------------------------------------------------------------------------
        /// get (lower case) root dir of the given file name
        // ------------------------------------------------------------------------
        public static string FindRootDirectory(string path)
        {
            if (path.EndsWith("\\"))
                path = path.Substring(0, path.Length - 1);

            while (path.Length > 0)
            {
              if (Directory.Exists(path + "\\.hg"))
                    return path;

                int index = path.LastIndexOf('\\');
                if (index >= 0)
                    path = path.Substring(0, index);
                else
                    break;
            }
            return string.Empty;
        }

        // ------------------------------------------------------------------------
        // get current used brunchname of repository
        // ------------------------------------------------------------------------
        public static string GetCurrentBranchName(string rootDirectory)
        {
          string branchName = "";

          List<string> resultList;
          HG.InvokeCommand(rootDirectory, "branch", out resultList);

          if (resultList.Count > 0)
            branchName = resultList[0];

          return branchName;
        }

        // ------------------------------------------------------------------------
        // update status dictionary with hg status cmd output
        // ------------------------------------------------------------------------
        public static bool UpdateStatusDictionary(List<string> lines, string rootDirectory, Dictionary<string, char> fileStatusDictionary, Dictionary<string, string> renamedToOrgFileDictionary)
        {
            Dictionary<string,string> copyRenamedFiles = new Dictionary<string,string>();
            
            char prevStatus = ' ';
            string prevFile = "";
            for(int pos=0; pos<lines.Count; ++pos)
            {
                string str  = lines[pos];
                char status = str[0];
                string file = rootDirectory + "\\" + str.Substring(2);

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

            foreach(var entry in copyRenamedFiles)
            {
                char orgFileStatus;
                if (!fileStatusDictionary.TryGetValue(entry.Key, out orgFileStatus))
                {
                    Dictionary<string, char> fileStatusOriginalFile;
                    string[] fileList = { entry.Key};
                    if(QueryFileStatus(fileList, out fileStatusOriginalFile))
                        fileStatusOriginalFile.TryGetValue(entry.Key, out orgFileStatus);
                }

                if (orgFileStatus=='R')
                    fileStatusDictionary[entry.Value] = 'N';
                else
                    fileStatusDictionary[entry.Value] = 'P';    
            }

            return true;
        }

        // ------------------------------------------------------------------------
        // run a HG root status query and get the resulting status informations to a fileStatusDictionary
        // ------------------------------------------------------------------------
        public static bool QueryRootStatus(string rootDirectory, out Dictionary<string, char> fileStatusDictionary)
        {
            Trace.WriteLine("Start QueryRootStatus");

            fileStatusDictionary = null;
            
            if (rootDirectory != string.Empty)
            {
                fileStatusDictionary = new Dictionary<string, char>();

                // Start a new process for the cmd
                Process process = HG.InvokeCommand(rootDirectory, "status -m -a -r -d -c -C ");

                List<string> lines = new List<string>();

                string str = "";
                while (!process.HasExited)
                {
                    while ((str = process.StandardOutput.ReadLine()) != null)
                    {
                        lines.Add(str);
                    }
                    Thread.Sleep(0);
                }

                while ((str = process.StandardOutput.ReadLine()) != null)
                {
                    lines.Add(str);
                }

                Dictionary<string, string> renamedToOrgFileDictionary = new Dictionary<string, string>();
                UpdateStatusDictionary(lines, rootDirectory, fileStatusDictionary, renamedToOrgFileDictionary);
            }
            return (fileStatusDictionary != null);
        }

        // ------------------------------------------------------------------------
        // query the files status and get them to the fileStatusDictionary
        // ------------------------------------------------------------------------
        static public bool QueryFileStatus(string[] fileList, out Dictionary<string, char> fileStatusDictionary, out Dictionary<string, string> renamedToOrgFileDictionary)
        {
            fileStatusDictionary = new Dictionary<string, char>();
            renamedToOrgFileDictionary = new Dictionary<string, string>();
            Dictionary<string, string> commandLines = new Dictionary<string, string>();

            try
            {
                if (fileList.Length > 0)
                {
                    for (int iFile = 0; iFile < fileList.Length; ++iFile)
                    {
                        string file = fileList[iFile];
                        string rootDirectory = HG.FindRootDirectory(file);

                        string commandLine = "";
                        commandLines.TryGetValue(rootDirectory, out commandLine);
                        commandLine += " \"" + file.Substring(rootDirectory.Length + 1) + "\" ";
                        
                        if (commandLine.Length>=(2000))
                        {
                            List<string> resultList;
                            InvokeCommand(rootDirectory, "status -A " + commandLine, out resultList);
                            UpdateStatusDictionary(resultList, rootDirectory, fileStatusDictionary, renamedToOrgFileDictionary);

                            // reset cmd line and filecounter for the next run
                            commandLine = "";
                        }

                        commandLines[rootDirectory] = commandLine;
                    }

                    foreach (KeyValuePair<string, string> directoryCommandLine in commandLines)
                    {
                        string rootDirectory = directoryCommandLine.Key;
                        string commandLine = directoryCommandLine.Value;

                        List<string> resultList;
                        InvokeCommand(rootDirectory, "status -A " + commandLine, out resultList);
                        UpdateStatusDictionary(resultList, rootDirectory, fileStatusDictionary, renamedToOrgFileDictionary);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("HGProcess.QueryFileStatus: " + ex.Message);
                return false;
            }

            return (fileStatusDictionary != null);
        }
        
        // ------------------------------------------------------------------------
        // query the files status and get them to the fileStatusDictionary
        // ------------------------------------------------------------------------
        static public bool QueryFileStatus(string[] fileList, out Dictionary<string, char> fileStatusDictionary)
        {
            Dictionary<string, string> renamedToOrgFileDictionary;
            return QueryFileStatus(fileList, out fileStatusDictionary, out renamedToOrgFileDictionary);
        }

        // ------------------------------------------------------------------------
        // put file under source control
        // ------------------------------------------------------------------------
        static public bool AddFiles(string[] fileList, out Dictionary<string, char> fileStatusDictionary)
        {
            return InvokeCommandGetStatus("add", fileList, out fileStatusDictionary);
        }

        // ------------------------------------------------------------------------
        // add files to the repositiry if they are not on the ignore list
        // ------------------------------------------------------------------------
        static public bool AddFilesNotIgnored(string[] fileList, out Dictionary<string, char> fileStatusDictionary)
        {
            List<string> addFilesList = new List<string>();

            Dictionary<string, char> statusDictionary;
            QueryFileStatus(fileList, out statusDictionary);
            foreach (var k in statusDictionary)
            {
              if (k.Value == '?' || k.Value == 'R')
              {
                addFilesList.Add(k.Key);
              }
            }

            if (addFilesList.Count>0)
                return InvokeCommandGetStatus("add", addFilesList.ToArray(), out fileStatusDictionary);
            
            fileStatusDictionary = null;
            return false;
        }

        // ------------------------------------------------------------------------
        // enter file renamed to hg repository
        // ------------------------------------------------------------------------
        static public bool EnterFileRenamed(string[] orgFileName, string[] newFileName)
        {
            try
            {
                for (int pos = 0; pos < orgFileName.Length; ++pos)
                {
                    string workingDirectory = orgFileName[pos].Substring(0, orgFileName[pos].LastIndexOf('\\'));
                    string rootDirectory = HG.FindRootDirectory(workingDirectory);

                    string ofile = orgFileName[pos].Substring(rootDirectory.Length + 1);
                    string nfile = newFileName[pos].Substring(rootDirectory.Length + 1);
                    List<string> list;
                    HG.InvokeCommand(rootDirectory,
                        "rename  -A \"" + ofile + "\" \"" + nfile + "\"", out list);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("HG.EnterFileRenamed exception- " + ex.Message);
                return false;
            }

            return true;
        }

        // ------------------------------------------------------------------------
        // enter file removed to hg repository
        // ------------------------------------------------------------------------
        static public bool EnterFileRemoved(string[] fileList, out Dictionary<string, char> fileStatusDictionary)
        {
            return InvokeCommandGetStatus("remove", fileList, out fileStatusDictionary);
        }


        // ------------------------------------------------------------------------
        // revert uncommited changes
        // ------------------------------------------------------------------------
        public static bool Revert(string[] fileList, out Dictionary<string, char> fileStatusDictionary)
        {
            return InvokeCommandGetStatus("revert", fileList, out fileStatusDictionary);
        }

        /// <summary>
        /// get original filename
        /// </summary>
        /// <param name="renamedFileName"></param>
        /// <returns></returns>
        public static string GetOriginalOfRenamedFile(string renamedFileName)
        {
            string orgFileNAme = string.Empty;
            string[] fileList = { renamedFileName };
            Dictionary<string, char> fileStatusDictionary;
            Dictionary<string, string> renamedToOrgFileDictionary;
            if (QueryFileStatus(fileList, out fileStatusDictionary, out renamedToOrgFileDictionary))
            {
                renamedToOrgFileDictionary.TryGetValue(renamedFileName.ToLower(), out orgFileNAme);
            }
            return orgFileNAme;
        }
    }
}
