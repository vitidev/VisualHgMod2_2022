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
        // invoke HGTK exe commands
        // ------------------------------------------------------------------------
        static Process InvokeCommand(string workingDirectory, string arguments)
        {
            if (workingDirectory != null && workingDirectory != string.Empty)
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = "HGTK.exe";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.Start();
                return process;
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
        // show TortoiseHG log dialog
        // ------------------------------------------------------------------------
        static public void LogDialog(string directory, string filter)
        {
            if (filter == string.Empty)
                HGTKDialog(directory, "log");
            else
                HGTKDialog(directory, "log " + filter);
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

        // ------------------------------------------------------------------------
        // show TortoiseHG datamine dialog
        // ------------------------------------------------------------------------
        static public void DataMineDialog(string directory)
        {
            HGTKDialog(directory, "datamine");
        }
    }
}
