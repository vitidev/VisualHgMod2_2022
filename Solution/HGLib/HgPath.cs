using System.IO;
using Microsoft.Win32;

namespace HgLib
{
    public static class HgPath
    {
        private static readonly string TortoiseHgRegistryKey = @"SOFTWARE\TortoiseHg";

        private static string _tortoiseHgDirectory;
        private static string _tortoiseHgExecutablePath;
        private static string _hgExecutablePath;
        private static string _kdiffExecutablePath;


        public static string TortoiseHgDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_tortoiseHgDirectory))
                {
                    using (var key = OpenTortoiseHgRegistryKey())
                    {
                        _tortoiseHgDirectory = key != null ? (string)key.GetValue(null) : "";
                    }
                }

                return _tortoiseHgDirectory ?? "";
            }
        }

        public static string TortoiseHgExecutable
        {
            get
            {
                if (string.IsNullOrEmpty(_tortoiseHgExecutablePath))
                {
                    var hgDir = TortoiseHgDirectory;

                    var thg = Path.Combine(hgDir, "thg.exe");
                    var hgtk = Path.Combine(hgDir, "hgtk.exe");

                    _tortoiseHgExecutablePath = File.Exists(thg) ? thg : File.Exists(hgtk) ? hgtk : thg;
                }

                return _tortoiseHgExecutablePath;
            }
        }

        public static string HgExecutable
        {
            get
            {
                if (string.IsNullOrEmpty(_hgExecutablePath))
                    _hgExecutablePath = Path.Combine(TortoiseHgDirectory, "hg.exe");

                return _hgExecutablePath;
            }
        }

        public static string KDiffExecutable
        {
            get
            {
                if (string.IsNullOrEmpty(_kdiffExecutablePath))
                    _kdiffExecutablePath = Path.Combine(TortoiseHgDirectory, "kdiff3.exe");

                return _kdiffExecutablePath;
            }
        }

        private static RegistryKey OpenTortoiseHgRegistryKey()
        {
            return
                Registry.CurrentUser.OpenSubKey(TortoiseHgRegistryKey) ??
                Registry.LocalMachine.OpenSubKey(TortoiseHgRegistryKey);
        }


        public static bool IsDirectory(string path)
        {
            if (File.Exists(path) || Directory.Exists(path))
                return File.GetAttributes(path).HasFlag(FileAttributes.Directory);

            return false;
        }

        public static string FindRepositoryRoot(string path)
        {
            while (!string.IsNullOrEmpty(path))
            {
                if (Directory.Exists(Path.Combine(path, ".hg")))
                    break;

                path = GetParentDirectory(path);
            }

            return path;
        }

        private static string GetParentDirectory(string path)
        {
            DirectoryInfo? parent;

            try
            {
                parent = Directory.GetParent(path);
            }
            catch
            {
                parent = null;
            }

            return parent != null ? parent.ToString() : "";
        }

        public static string StripRoot(string fileName, string root)
        {
            return fileName.Substring(root.Length + 1);
        }

        public static string GetRandomTemporaryFileName()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }
    }
}