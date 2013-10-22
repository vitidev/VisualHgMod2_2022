using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace HGLib
{
    // ------------------------------------------------------------------------
    // wrapper for HGTK.exe dialogs as commiting changes, viewing status,
    // history logs, update to rev ...
    // ------------------------------------------------------------------------
    public static class HGTK
    {
      // ------------------------------------------------------------------------
      // tortois hgtk.exe file
      // ------------------------------------------------------------------------
      static string hgtkexe = null;
      public static string GetHGTKFileName()
      {
        if (hgtkexe == null || hgtkexe == string.Empty)
        {
          hgtkexe = Hg.GetTortoiseHgDirectory();
          if (hgtkexe != null && hgtkexe != string.Empty)
          {

            if (File.Exists(Path.Combine(hgtkexe, "HGTK.exe")))
              hgtkexe = Path.Combine(hgtkexe, "HGTK.exe");
            else if (File.Exists(Path.Combine(hgtkexe, "THG.exe")))
              hgtkexe = Path.Combine(hgtkexe, "THG.exe");
          }
        }
        return hgtkexe;
      }

      // ------------------------------------------------------------------------
      // invoke arbitrary command
      // ------------------------------------------------------------------------
      static Process InvokeCommand(string executable, string workingDirectory, string arguments)
      {
        Process process = new Process();
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.FileName = executable;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.Start();
        return process;
      }

      // ------------------------------------------------------------------------
        // invoke HGTK exe commands
        // ------------------------------------------------------------------------
        static Process InvokeCommand(string workingDirectory, string arguments)
        {
          if (workingDirectory != null && workingDirectory != string.Empty)
          {
            return InvokeCommand(GetHGTKFileName(), workingDirectory, arguments); 
          }
          
          return null;  
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG dialog
        // ------------------------------------------------------------------------
        public static Process HGTKDialog(string directory, string dialog)
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
                // TortoiseHG HGTK commit dialog exception
                Trace.WriteLine("HGTK " + dialog + " dialog exception " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Replace the following token
        /// $(ProgramFiles)
        /// $(ProgramFiles (x86))
        /// $(Base)
        /// $(Mine)
        /// $(BaseName)
        /// $(MineName)
        /// </summary>
        /// <param name="versionedFile"></param>
        /// <param name="currentFile"></param>
        /// <param name="commandMask"></param>
        /// <returns></returns>
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
            command = command.Replace("$(BaseName)", Path.GetFileName(versionedFile) );
            command = command.Replace("$(MineName)", Path.GetFileName(currentFile) );
            return command;
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG status dialog
        // ------------------------------------------------------------------------
        static public Process DiffDialog(string sccFile, string file, string commandMask)
        {
          String root = HGLib.Hg.FindRepositoryRoot(file); 
          if(root != String.Empty)
          {
            // copy latest file revision from repo temp folder
            string currentFile = file;
            string versionedFile = Path.GetTempPath() + sccFile.Substring(sccFile.LastIndexOf("\\") + 1) + "(base)";
            
            // delete file if exists
            File.Delete(versionedFile);

            string cmd = "cat \"" + sccFile.Substring(root.Length + 1) + "\"  -o \"" + versionedFile + "\"";
            InvokeCommand(Hg.GetHgExecutablePath(), root, cmd);
            
            // wait file exists on disk
            int counter = 0;
            while(!File.Exists(versionedFile) && counter < 10)
            { Thread.Sleep(100); ++counter; }
            
            // run diff tool
            if (commandMask != string.Empty)
            {
                cmd = PrepareDiffCommand(versionedFile, currentFile, commandMask);
                return InvokeCommand(cmd, "", "");
            }
            else
            {
                commandMask = " \"$(Base)\" --fname \"$(BaseName)\" \"$(Mine)\" --fname \"$(MineName)\" ";
                cmd = PrepareDiffCommand(versionedFile, currentFile, commandMask); 
                return InvokeCommand(Path.Combine(Hg.GetTortoiseHgDirectory(), "kdiff3.exe"), root, cmd);
            }
          }
          return null;
        }
        
        // ------------------------------------------------------------------------
        // show TortoiseHG clone dialog
        // ------------------------------------------------------------------------
        static public void CloneDialog(string directory)
        {
            HGTKDialog(directory, "clone");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG merge dialog
        // ------------------------------------------------------------------------
        static public void MergeDialog(string directory)
        {
            HGTKDialog(directory, "merge");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG update dialog
        // ------------------------------------------------------------------------
        static public void UpdateDialog(string directory)
        {
            HGTKDialog(directory, "update");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG user configuration dialog
        // ------------------------------------------------------------------------
        static public void ConfigDialog(string directory)
        {
            HGTKDialog(directory, "userconfig");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG recovery dialog
        // ------------------------------------------------------------------------
        static public void RecoveryDialog(string directory)
        {
            HGTKDialog(directory, "recovery");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG datamine dialog
        // ------------------------------------------------------------------------
        static public void DataMineDialog(string directory)
        {
            HGTKDialog(directory, "datamine");
        }

        // command " --nofork revert "
        public static void HGTKSelectedFilesDialog(string[] files, string command)
        {
            string tmpFile = Hg.TemporaryFile;
            StreamWriter stream = new StreamWriter(tmpFile, false, Encoding.Default);

            string currentRoot = string.Empty;
            for (int n = 0; n < files.Length; ++n)
            {
                string root = HGLib.Hg.FindRepositoryRoot(files[n]);
                if (root == string.Empty)
                    continue;

                if (currentRoot == string.Empty)
                {
                    currentRoot = root;
                }
                else if (string.Compare(currentRoot, root, true) != 0)
                {
                    stream.Close();
                    Process process = HGTKDialog(root, command + " --listfile \"" + tmpFile + "\"");
                    process.WaitForExit();

                    tmpFile = Hg.TemporaryFile;
                    stream = new StreamWriter(tmpFile, false, Encoding.Default);
                }

                stream.WriteLine(files[n]);
            }

            stream.Close();
            if (currentRoot != string.Empty)
            {
                Process process2 = HGTKDialog(currentRoot, command + " --listfile \"" + tmpFile + "\"");
                process2.WaitForExit();
            }
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG datamine dialog
        // ------------------------------------------------------------------------
        static public void AnnotateDialog(string file)
        {
            String root = HGLib.Hg.FindRepositoryRoot(file);
            if(root != String.Empty)
            {
                file = file.Substring(root.Length + 1);
                HGTKDialog(root, "annotate \"" + file + "\"");
            }
        }
    }
}
