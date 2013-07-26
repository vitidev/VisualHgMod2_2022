using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace HgLib
{
    // ------------------------------------------------------------------------
    // wrapper for HgTK.exe dialogs as commiting changes, viewing status,
    // history logs, update to rev ...
    // ------------------------------------------------------------------------
    public static class HgTK
    {
      // ------------------------------------------------------------------------
      // tortois hgtk.exe file
      // ------------------------------------------------------------------------
      static string hgtkexe = null;
      public static string GetHgTKFileName()
      {
        if (hgtkexe == null || hgtkexe == string.Empty)
        {
          hgtkexe = Hg.GetTortoiseHgDirectory();
          if (hgtkexe != null && hgtkexe != string.Empty)
          {

            if (File.Exists(Path.Combine(hgtkexe, "HgTK.exe")))
              hgtkexe = Path.Combine(hgtkexe, "HgTK.exe");
            else if (File.Exists(Path.Combine(hgtkexe, "THg.exe")))
              hgtkexe += Path.Combine(hgtkexe, "THg.exe");
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
        // invoke HgTK exe commands
        // ------------------------------------------------------------------------
        static Process InvokeCommand(string workingDirectory, string arguments)
        {
          if (workingDirectory != null && workingDirectory != string.Empty)
          {
            return InvokeCommand(GetHgTKFileName(), workingDirectory, arguments); 
          }
          
          return null;  
        }

        // ------------------------------------------------------------------------
        // show TortoiseHg dialog
        // ------------------------------------------------------------------------
        public static Process HgTKDialog(string directory, string dialog)
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
        // show TortoiseHg status dialog
        // ------------------------------------------------------------------------
        static public Process DiffDialog(string sccFile, string file, string commandMask)
        {
          String root = HgLib.Hg.FindRepositoryRoot(file); 
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
        // show TortoiseHg clone dialog
        // ------------------------------------------------------------------------
        static public void CloneDialog(string directory)
        {
            HgTKDialog(directory, "clone");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHg merge dialog
        // ------------------------------------------------------------------------
        static public void MergeDialog(string directory)
        {
            HgTKDialog(directory, "merge");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHg update dialog
        // ------------------------------------------------------------------------
        static public void UpdateDialog(string directory)
        {
            HgTKDialog(directory, "update");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHg user configuration dialog
        // ------------------------------------------------------------------------
        static public void ConfigDialog(string directory)
        {
            HgTKDialog(directory, "userconfig");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHg recovery dialog
        // ------------------------------------------------------------------------
        static public void RecoveryDialog(string directory)
        {
            HgTKDialog(directory, "recovery");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHg datamine dialog
        // ------------------------------------------------------------------------
        static public void DataMineDialog(string directory)
        {
            HgTKDialog(directory, "datamine");
        }

        // command " --nofork revert "
        public static void HgTKSelectedFilesDialog(string[] files, string command)
        {
            string tmpFile = Hg.TemporaryFile;
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

                    tmpFile = Hg.TemporaryFile;
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

        // ------------------------------------------------------------------------
        // show TortoiseHg datamine dialog
        // ------------------------------------------------------------------------
        static public void AnnotateDialog(string file)
        {
            String root = HgLib.Hg.FindRepositoryRoot(file);
            if(root != String.Empty)
            {
                file = file.Substring(root.Length + 1);
                HgTKDialog(root, "annotate \"" + file + "\"");
            }
        }
    }
}
