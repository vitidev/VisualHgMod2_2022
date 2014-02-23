using System;
using System.ComponentModel;
using System.Diagnostics;

namespace VisualHg
{
    public class DiffTool
    {
        public string FileName { get; set; }

        public string Arguments { get; set; }


        public event EventHandler Exited = (s, e) => { };


        public virtual void Start(string fileA, string fileB, string nameA, string nameB, string workingDirectory)
        {
            var process = new Process();

            process.Exited += (s, e) => OnExited();

            process.StartInfo.FileName = FileName;
            process.StartInfo.Arguments = GetArguments(fileA, fileB, nameA, nameB);
            process.StartInfo.WorkingDirectory = workingDirectory;

            try
            {
                process.Start();
            }
            catch (Win32Exception e)
            {
                OnExited();
                
                throw new InvalidOperationException("Diff tool start failed", e);
            }
        }

        private string GetArguments(string fileA, string fileB, string nameA, string nameB)
        {
            return Arguments
                .Replace("%PathA%", String.Concat('"', fileA, '"'))
                .Replace("%PathB%", String.Concat('"', fileB, '"'))
                .Replace("%NameA%", String.Concat('"', nameA, '"'))
                .Replace("%NameB%", String.Concat('"', nameB, '"'));
        }

        private void OnExited()
        {
            Exited(this, EventArgs.Empty);
        }
    }
}
