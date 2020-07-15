using HgLib;
using Microsoft.VisualStudio.Shell;

namespace VisualHg
{
    public static class VisualHgFileStatus
    {
        public static bool IsNotAdded(string fileName)
        {
            return Matches(fileName, HgFileStatus.NotAdded);
        }

        public static bool IsPending(string fileName)
        {
            return Matches(fileName, HgFileStatus.Pending);
        }

        public static bool Matches(string fileName, HgFileStatus pattern)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            if (HgPath.IsDirectory(fileName))
                return false;

            var visualHgService = (VisualHgService)Package.GetGlobalService(typeof(VisualHgService));

            var status = visualHgService.GetFileStatus(fileName);

            return Matches(status, pattern);
        }

        public static bool Matches(HgFileStatus status, HgFileStatus pattern)
        {
            return (status & pattern) > 0;
        }
    }
}