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
            return InvokeCommand("HGTK.exe", workingDirectory, arguments); 
          }
          
          return null;  
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG commit dialog
        // ------------------------------------------------------------------------
        static void HGTKDialog(string directory, string dialog)
        {
            try
            {
                while (!Directory.Exists(directory) && directory.Length > 0)
                {
                    directory = directory.Substring(0, directory.LastIndexOf('\\'));
                }
                InvokeCommand(directory, dialog);
            }
            catch (Exception ex)
            {
                // TortoiseHG HGTK commit dialog exception
                Trace.WriteLine("HGTK " + dialog + " dialog exception " + ex.Message);
            }
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG repo browser dialog
        // ------------------------------------------------------------------------
        static public void RepoBrowserDialog(string root)
        {
            HGTKDialog(root, "log");
        }

        
        // ------------------------------------------------------------------------
        // show TortoiseHG file log dialog
        // ------------------------------------------------------------------------
        static public void LogDialog(string file)
        {
            String root = HGLib.HG.FindRootDirectory(file);
            if (root != string.Empty)
            { 
                file = file.Substring(root.Length + 1);
                HGTKDialog(root, "log \"" + file + "\"");
            }
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG synchronize dialog
        // ------------------------------------------------------------------------
        static public void SyncDialog(string directory)
        {
            HGTKDialog(directory, "synch");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG status dialog
        // ------------------------------------------------------------------------
        static public void StatusDialog(string directory)
        {
            HGTKDialog(directory, "status");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG status dialog
        // ------------------------------------------------------------------------
        static public void DiffDialog(string sccFile, string file)
        {
          String root = HGLib.HG.FindRootDirectory(file); 
          if(root != String.Empty)
          {
            // copy latest file revision from repo temp folder
            string currentFile = file;
            string versionedFile = Path.GetTempPath() + sccFile.Substring(sccFile.LastIndexOf("\\") + 1) + "(base)";
            string cmd = "cat \"" + sccFile.Substring(root.Length + 1) + "\"  -o \"" + versionedFile + "\"";
            InvokeCommand("hg.exe", root, cmd);
            
            // wait file exists on disk
            int counter = 0;
            while(!File.Exists(versionedFile) && counter < 10)
            { Thread.Sleep(1); ++counter; }
            
            // run diff tool
            cmd = "\"" + versionedFile + "\" \"" + currentFile + "\"";
            InvokeCommand(HGSetup.GetDiffTool(root), root, cmd);
          }
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
        // show TortoiseHG commit dialog
        // ------------------------------------------------------------------------
        static public void CommitDialog(string directory)
        {
            HGTKDialog(directory, "commit");
        }
        
        /// <summary>
        /// commit dialog with preselected / filtered files
        /// </summary>
        /// <param name="files"></param>
        static public void CommitDialog(string [] files)
        {
            StringBuilder stream = new StringBuilder();
            string currentRoot = string.Empty;
            for (int n = 0; n < files.Length; ++n)
            {
                string root = HGLib.HG.FindRootDirectory(files[n]);
                if (currentRoot == string.Empty)
                {
                    currentRoot = root;
                }
                else if (currentRoot != root)
                {
                    HGTKDialog(root, "commit " + stream);
                    stream.Clear();
                }

                string file = files[n].Substring(currentRoot.Length + 1);
                stream.AppendFormat(" \"{0}\"", file);
            }

            if (stream.Length > 0 )
                HGTKDialog(currentRoot, "commit " + stream);
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG datamine dialog
        // ------------------------------------------------------------------------
        static public void DataMineDialog(string directory)
        {
            HGTKDialog(directory, "datamine");
        }
        
        // ------------------------------------------------------------------------
        // show TortoiseHG revert dialog
        // ------------------------------------------------------------------------
        static public void RevertDialog(string file)
        {
            String root = HGLib.HG.FindRootDirectory(file);
            if(root != String.Empty)
            {
                file = file.Substring(root.Length + 1); 
                HGTKDialog(root, "revert \"" + file + "\"");
            }
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG datamine dialog
        // ------------------------------------------------------------------------
        static public void AnnotateDialog(string file)
        {
            String root = HGLib.HG.FindRootDirectory(file);
            if(root != String.Empty)
            {
                file = file.Substring(root.Length + 1);
                HGTKDialog(root, "annotate \"" + file + "\"");
            }
        }
    }
}
