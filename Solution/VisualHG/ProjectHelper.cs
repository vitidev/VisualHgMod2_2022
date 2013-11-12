using System;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    public class ProjectHelper
    {
        public static string GetDirectoryName(IVsHierarchy hierarchy)
        {
            object name = null;

            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir, out name);

            var directory = name as string;

            return !String.IsNullOrEmpty(directory) ? GetNormalizedFullPath(directory) : "";
        }

        private static string GetNormalizedFullPath(string path)
        {
            path = Path.GetFullPath(path);

            if (path.Length >= 2 && path[1] == ':')
            {
                var driveLetter = path[0];

                if ((driveLetter >= 'a') && (driveLetter <= 'z'))
                {
                    path = driveLetter.ToString().ToUpperInvariant() + path.Substring(1);
                }

                var r = path.TrimEnd('\\');

                if (r.Length > 3)
                {
                    return r;
                }

                return path.Substring(0, 3);
            }
            else if (path.StartsWith(@"\\"))
            {
                var root = Path.GetPathRoot(path).ToLowerInvariant();

                if (!path.StartsWith(root))
                {
                    path = root + path.Substring(root.Length).TrimEnd('\\');
                }
            }

            return path.TrimEnd('\\');
        }
    }
}
