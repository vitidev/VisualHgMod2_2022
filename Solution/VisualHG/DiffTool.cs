using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace VisualHg
{
    public class DiffTool
    {
        public string FileName { get; set; }

        public string Arguments { get; set; }


        public event EventHandler Exited = (s, e) => { };


        public void Start(string fileA, string fileB, string workingDirectory)
        {
            var process = new Process();

            process.Exited += (s, e) => OnExited();

            process.StartInfo.FileName = FileName;
            process.StartInfo.Arguments = GetArguments(fileA, fileB);
            process.StartInfo.WorkingDirectory = workingDirectory;

            try
            {
                process.Start();
            }
            catch (Win32Exception)
            {
                OnExited();
            }
        }

        private string GetArguments(string fileA, string fileB)
        {
            return Arguments
                .Replace("%PathA%", String.Concat('"', fileA, '"'))
                .Replace("%PathB%", String.Concat('"', fileB, '"'))
                .Replace("%NameA%", String.Concat('"', Path.GetFileName(fileA), '"'))
                .Replace("%NameB%", String.Concat('"', Path.GetFileName(fileB), '"'));
        }

        private void OnExited()
        {
            Exited(this, EventArgs.Empty);
        }
    }
}
