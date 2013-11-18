using System;
using System.Threading;
using HgLib;

namespace VisualHg
{
    partial class SccProvider
    {
        public void ShowCommitWindow(string directory)
        {
            StartTortoiseHg("commit", directory);
        }

        public void ShowWorkbenchWindow(string directory)
        {
            StartTortoiseHg("log", directory);
        }

        public void ShowStatusWindow(string directory)
        {
            StartTortoiseHg("status", directory);
        }

        public void ShowSynchronizeWindow(string directory)
        {
            StartTortoiseHg("synch", directory);
        }

        public void ShowUpdateWindow(string directory)
        {
            StartTortoiseHg("update", directory);
        }


        public void ShowAddSelectedWindow(string[] files)
        {
            StartTortoiseHg(" --nofork add ", files);
        }

        public void ShowCommitWindowPrivate(string[] files)
        {
            StartTortoiseHg(" --nofork commit ", files);
        }

        public void ShowDiffWindow(string parent, string current, string customDiffTool)
        {
            StartDiff(parent, current, customDiffTool);
        }

        public void ShowRevertWindowPrivate(string[] files)
        {
            StartTortoiseHg(" --nofork revert ", files);
        }

        public void ShowHistoryWindowPrivate(string fileName)
        {
            var root = HgPath.FindRepositoryRoot(fileName);

            if (!String.IsNullOrEmpty(root))
            {
                fileName = fileName.Substring(root.Length + 1);

                StartTortoiseHg(String.Format("log \"{0}\"", fileName), root);
            }
        }


        private void StartTortoiseHg(string command, string directory)
        {
            TortoiseHg.Start(command, directory);
        }

        private void StartTortoiseHg(string command, string[] files)
        {
            TortoiseHg.StartForEachRoot(command, files);
        }

        private void StartDiff(string parent, string current, string customDiffTool)
        {
            TortoiseHg.StartDiff(parent, current, customDiffTool);
        }
    }
}
