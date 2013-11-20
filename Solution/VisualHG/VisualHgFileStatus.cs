using System;
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
            if (String.IsNullOrEmpty(fileName))
            {
                return false;
            }

            if (HgPath.IsDirectory(fileName))
            {
                return false;
            }

            var visualHgService = Package.GetGlobalService(typeof(VisualHgService)) as VisualHgService;
            
            var status = visualHgService.GetFileStatus(fileName);

            return (status & pattern) > 0;
        }
    };
}
