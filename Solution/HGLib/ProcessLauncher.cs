using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace HgLib
{
    internal static class ProcessLauncher
    {
        internal static string[] RunHg(string args, string workingDirectory)
        {
            return ReadOutputFrom(StartHg(args, workingDirectory));
        }
        
        internal static string[] RunTortoiseHg(string args, string workingDirectory)
        {
            return ReadOutputFrom(StartTortoiseHg(args, workingDirectory));
        }


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

            try
            {
                process.Start();
            }
            catch (Win32Exception) { }

            return process;
        }


        private static string[] ReadOutputFrom(Process process)
        {
            var outputLines = new List<string>();

            try
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    outputLines.Add(process.StandardOutput.ReadLine());
                }
            }
            catch (InvalidOperationException) { }

            return outputLines.ToArray();
        }
    }
}
