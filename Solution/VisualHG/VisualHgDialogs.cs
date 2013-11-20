using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HgLib;

namespace VisualHg
{
    public static class VisualHgDialogs
    {
        public static void ShowCommitWindow(string[] files)
        {
            var filesToCommit = files.Where(VisualHgFileStatus.IsPending).ToArray();

            if (filesToCommit.Length > 0)
            {
                TortoiseHg.ShowCommitWindow(filesToCommit);
            }
        }

        public static void ShowRevertWindow(string[] files)
        {
            var filesToRevert = files.Where(VisualHgFileStatus.IsPending).ToArray();

            if (filesToRevert.Length > 0)
            {
                TortoiseHg.ShowRevertWindow(filesToRevert);
            }
        }

        public static void ShowHistoryWindow(string fileName)
        {
            var originalFileName = GetOriginalFileName(fileName);

            TortoiseHg.ShowHistoryWindow(originalFileName);
        }


        public static void ShowDiffWindow(string fileName)
        {
            var root = HgPath.FindRepositoryRoot(fileName);
            var parent = GetOriginalFileName(fileName);
            var temp = Hg.CreateParentRevisionTempFile(parent, root);
            
            var diffTool = GetDiffTool();

            diffTool.Exited += (s, e) => DeleteFile(temp);

            diffTool.Start(temp, fileName, root);
        }

        private static void DeleteFile(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch { }
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


        private static string GetOriginalFileName(string fileName)
        {
            if (VisualHgFileStatus.Matches(fileName, HgFileStatus.Renamed | HgFileStatus.Copied))
            {
                return Hg.GetRenamedFileOriginalName(fileName);
            }

            return fileName;
        }
    }
}