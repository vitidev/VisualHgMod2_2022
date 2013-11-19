using System;
using System.Linq;
using System.Windows.Forms;
using HgLib;

namespace VisualHg
{
    partial class SccProvider
    {
        public void ShowCommitWindow(string[] files)
        {
            SaveAllFiles();

            var filesToCommit = files.Where(FileIsPending).ToArray();

            if (filesToCommit.Length > 0)
            {
                TortoiseHg.ShowCommitWindow(filesToCommit);
            }
        }

        public void ShowDiffWindow(string fileName)
        {
            SaveAllFiles();

            if (String.IsNullOrEmpty(fileName))
            {
                return;
            }

            var parent = GetOriginalFileName(fileName);

            if (String.IsNullOrEmpty(parent))
            {
                return;
            }

            try
            {
                TortoiseHg.ShowDiffWindow(parent, fileName, Configuration.Global.ExternalDiffToolCommandMask);
            }
            catch
            {
                if (!String.IsNullOrEmpty(Configuration.Global.ExternalDiffToolCommandMask))
                {
                    MessageBox.Show("The DiffTool raised an error\nPlease check your command mask:\n\n" + Configuration.Global.ExternalDiffToolCommandMask, "VisualHg", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        public void ShowRevertWindow(string[] files)
        {
            SaveAllFiles();

            var filesToRevert = files.Where(FileIsPending).ToArray();

            if (filesToRevert.Length > 0)
            {
                TortoiseHg.ShowRevertWindow(filesToRevert.ToArray());
            }
        }

        public void ShowHistoryWindow(string fileName)
        {
            SaveAllFiles();

            var originalFileName = GetOriginalFileName(fileName);

            TortoiseHg.ShowHistoryWindow(originalFileName);
        }

        
        private string GetOriginalFileName(string fileName)
        {
            if (FileStatusMatches(fileName, HgFileStatus.Renamed | HgFileStatus.Copied))
            {
                return Hg.GetRenamedFileOriginalName(fileName);
            }

            return fileName;
        }
    }
}
