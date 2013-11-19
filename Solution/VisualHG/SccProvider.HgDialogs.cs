using System;
using System.IO;
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


        public void ShowDiffWindow(string fileName)
        {
            SaveAllFiles();

            var root = HgPath.FindRepositoryRoot(fileName);
            var parent = GetOriginalFileName(fileName);
            var temp = Hg.CreateParentRevisionTempFile(parent, root);
            
            var diffTool = GetDiffTool();

            diffTool.Exited += (s, e) => DeleteFile(temp);

            diffTool.Start(temp, fileName, root);
        }

        private static void DeleteFile(string file)
        {
            File.Delete(file);
        }

        private static DiffTool GetDiffTool()
        {
            if (String.IsNullOrEmpty(Configuration.Global.DiffToolPath))
            {
                return GetKDiff();
            }
         
            return new DiffTool
            {
                FileName = Configuration.Global.DiffToolPath,
                Arguments = Configuration.Global.DiffToolArguments,
            };
        }

        private static DiffTool GetKDiff()
        {
            var args = Configuration.Global.DiffToolArguments;

            if (String.IsNullOrEmpty(args))
            {
                args = "%PathA% --fname %NameA%  %PathB% --fname %NameB%";
            }

            return new DiffTool
            {
                FileName = HgPath.KDiffExecutable,
                Arguments = args,
            };
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