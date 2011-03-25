using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace VisualHG
{
    partial class SccProvider
    {
        // ------------------------------------------------------------------------
        // show an wait for exit of required dialog
        // update state for given files
        // ------------------------------------------------------------------------
        void QueueDialog(string[] files, string command)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                HGLib.HGTK.HGTKSelectedFilesDialog(files, command);
                sccService.StatusTracker.RebuildStatusCacheRequiredFlag=false;
                sccService.StatusTracker.AddWorkItem(new HGLib.UpdateFileStatusCommand(files));
            });
        }

        // ------------------------------------------------------------------------
        // commit selected files dialog
        // ------------------------------------------------------------------------
        public void CommitDialog(string[] files)
        {
            QueueDialog(files, " --nofork commit ");
        }

        // ------------------------------------------------------------------------
        // add files to repo dialog
        // ------------------------------------------------------------------------
        void AddFilesDialog(string[] files)
        {
            QueueDialog(files, " --nofork add ");
        }

        // ------------------------------------------------------------------------
        // show an wait for exit of required dialog
        // update state for given files
        // ------------------------------------------------------------------------
        void QueueDialog(string root, string command)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Process process = HGLib.HGTK.HGTKDialog(root, command);
                if (process != null)
                    process.WaitForExit();

                sccService.StatusTracker.RebuildStatusCacheRequiredFlag = false;
                sccService.StatusTracker.AddWorkItem(new HGLib.UpdateRootStatusCommand(root));
            });
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG commit dialog
        // ------------------------------------------------------------------------
        void CommitDialog(string directory)
        {
            QueueDialog(directory, "commit");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG revert dialog
        // ------------------------------------------------------------------------
        void RevertDialog(string[] files)
        {
            QueueDialog(files, " --nofork revert ");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG repo browser dialog
        // ------------------------------------------------------------------------
        public void RepoBrowserDialog(string root)
        {
            QueueDialog(root, "log");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG file log dialog
        // ------------------------------------------------------------------------
        public void LogDialog(string file)
        {
            String root = HGLib.HG.FindRootDirectory(file);
            if (root != string.Empty)
            {
                file = file.Substring(root.Length + 1);
                QueueDialog(root, "log \"" + file + "\"");
            }
        }

        // ------------------------------------------------------------------------
        // show file diff window
        // ------------------------------------------------------------------------
        void DiffDialog(string sccFile, string file, string commandMask)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Process process= HGLib.HGTK.DiffDialog(sccFile, file, commandMask);
                if (process != null)
                    process.WaitForExit();

                sccService.StatusTracker.RebuildStatusCacheRequiredFlag = false;
                sccService.StatusTracker.AddWorkItem(new HGLib.UpdateFileStatusCommand(new string[]{file}));
            });
        }


        // ------------------------------------------------------------------------
        // show TortoiseHG synchronize dialog
        // ------------------------------------------------------------------------
        public void SyncDialog(string directory)
        {
            QueueDialog(directory, "synch");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHG status dialog
        // ------------------------------------------------------------------------
        public void StatusDialog(string directory)
        {
            QueueDialog(directory, "status");
        }
    }
}
