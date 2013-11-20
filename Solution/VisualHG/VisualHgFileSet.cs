using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    public class VisualHgFileSet
    {
        private HashSet<string> items;

        public object SyncRoot { get; private set; }


        public VisualHgFileSet()
        {
            items = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            SyncRoot = new object();
        }


        public string[] Add(IVsHierarchy hierarchy)
        {
            var files = GetFiles(hierarchy);

            lock (SyncRoot)
            {
                foreach (var fileName in files)
                {
                    items.Add(fileName);
                }
            }

            return files;
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                items.Clear();
            }
        }

        public bool Contains(string fileName)
        {
            lock (SyncRoot)
            {
                return items.Contains(fileName);
            }
        }

        public void Remove(IVsHierarchy hierarchy)
        {
            lock (SyncRoot)
            {
                if (items.Count > 0)
                {
                    Remove(GetFiles(hierarchy));
                }
            }
        }

        private void Remove(string[] fileNames)
        {
            lock (items)
            {
                foreach (var fileName in fileNames)
                {
                    items.Remove(fileName);
                }
            }
        }

        private static string[] GetFiles(IVsHierarchy hierarchy)
        {
            var project = hierarchy as IVsSccProject2;
            
            return SccProvider.GetProjectFiles(project);
        }
    }
}
