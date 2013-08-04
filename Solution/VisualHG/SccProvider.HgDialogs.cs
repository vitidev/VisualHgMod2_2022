using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace VisualHg
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
                try {
                HgLib.Hgtk.HgTKSelectedFilesDialog(files, command);
                sccService.StatusTracker.RebuildStatusCacheRequiredFlag=false;
                sccService.StatusTracker.AddWorkItem(new HgLib.UpdateFileStatusCommand(files));
                }catch{}
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
                try{
                Process process = HgLib.Hgtk.HgTKDialog(root, command);
                if (process != null)
                    process.WaitForExit();

                sccService.StatusTracker.RebuildStatusCacheRequiredFlag = false;
                sccService.StatusTracker.AddWorkItem(new HgLib.UpdateRootStatusCommand(root));
                }catch{}
            });
        }

        // ------------------------------------------------------------------------
        // show TortoiseHg commit dialog
        // ------------------------------------------------------------------------
        void CommitDialog(string directory)
        {
            QueueDialog(directory, "commit");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHg revert dialog
        // ------------------------------------------------------------------------
        void RevertDialog(string[] files)
        {
            QueueDialog(files, " --nofork revert ");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHg repo browser dialog
        // ------------------------------------------------------------------------
        public void RepoBrowserDialog(string root)
        {
            QueueDialog(root, "log");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHg file log dialog
        // ------------------------------------------------------------------------
        public void LogDialog(string file)
        {
            String root = HgLib.Hg.FindRepositoryRoot(file);
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
                try{
                Process process= HgLib.Hgtk.DiffDialog(sccFile, file, commandMask);
                if (process != null)
                    process.WaitForExit();

                sccService.StatusTracker.RebuildStatusCacheRequiredFlag = false;
                sccService.StatusTracker.AddWorkItem(new HgLib.UpdateFileStatusCommand(new string[]{file}));
                }catch{}
            });
        }


        // ------------------------------------------------------------------------
        // show TortoiseHg synchronize dialog
        // ------------------------------------------------------------------------
        public void SyncDialog(string directory)
        {
            QueueDialog(directory, "synch");
        }

        // ------------------------------------------------------------------------
        // show TortoiseHg status dialog
        // ------------------------------------------------------------------------
        public void StatusDialog(string directory)
        {
            QueueDialog(directory, "status");
        }
    }
}
