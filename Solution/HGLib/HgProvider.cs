using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace HgLib
{
    public static class HgProvider
    {
        private static string tortoiseHgDirectory;
        private static string tortoiseHgExecutablePath;
        private static string hgExecutablePath;
        private static string kdiffExecutablePath;


        public static string FindRepositoryRoot(string path)
        {
            while (!String.IsNullOrEmpty(path))
            {
                if (Directory.Exists(Path.Combine(path, ".hg")))
                {
                    break;
                }

                path = GetParentDirectory(path);
            }

            return path;
        }

        private static string GetParentDirectory(string path)
        {
            DirectoryInfo parent;

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


        internal static string GetTortoiseHgDirectory()
        {
            if (String.IsNullOrEmpty(tortoiseHgDirectory))
            {
                var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\TortoiseHg");

                if (key == null)
                {
                    key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\TortoiseHg");
                }

                if (key != null)
                {
                    tortoiseHgDirectory = (string)key.GetValue(null);
                }
            }

            return tortoiseHgDirectory ?? "";
        }
        
        internal static string GetTortoiseHgExecutablePath()
        {
            if (String.IsNullOrEmpty(tortoiseHgExecutablePath))
            {
                var hgDir = HgProvider.GetTortoiseHgDirectory();

                var thg = Path.Combine(tortoiseHgExecutablePath, "thg.exe");
                var hgtk = Path.Combine(tortoiseHgExecutablePath, "hgtk.exe");

                tortoiseHgExecutablePath = File.Exists(thg) ? thg : File.Exists(hgtk) ? hgtk : null;
            }

            return tortoiseHgExecutablePath;
        }

        internal static string GetHgExecutablePath()
        {
            if (String.IsNullOrEmpty(hgExecutablePath))
            {
                hgExecutablePath = Path.Combine(GetTortoiseHgDirectory(), "hg.exe");
            }

            return hgExecutablePath;
        }

        internal static string GetKDiffExecutablePath()
        {
            if (String.IsNullOrEmpty(kdiffExecutablePath))
            {
                kdiffExecutablePath = Path.Combine(GetTortoiseHgDirectory(), "kdiff3.exe");
            }

            return kdiffExecutablePath;
        }


        internal static Process StartTortoiseHg(string args, string workingDirectory)
        {
            return Start(GetTortoiseHgExecutablePath(), workingDirectory, args);
        }

        internal static Process StartHg(string args, string workingDirectory)
        {
            return Start(GetHgExecutablePath(), args, workingDirectory);
        }

        internal static Process StartKDiff(string args, string workingDirectory)
        {
            return Start(GetKDiffExecutablePath(), args, workingDirectory);
        }

        internal static Process Start(string executable, string args, string workingDirectory)
        {
            var process = new Process();

            process.StartInfo.Arguments = args;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = executable;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = workingDirectory;

            process.Start();

            return process;
        }   
    }
}
