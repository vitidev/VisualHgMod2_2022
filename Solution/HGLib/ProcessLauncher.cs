using System.Diagnostics;

namespace HgLib
{
    internal static class ProcessLauncher
    {
        internal static Process StartTortoiseHg(string args, string workingDirectory)
        {
            return Start(HgPath.TortoiseHgExecutable, args, workingDirectory);
        }

        internal static Process StartHg(string args, string workingDirectory)
        {
            return Start(HgPath.HgExecutable, args, workingDirectory);
        }

        internal static Process StartKDiff(string args, string workingDirectory)
        {
            return Start(HgPath.KDiffExecutable, args, workingDirectory);
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
