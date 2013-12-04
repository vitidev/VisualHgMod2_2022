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


        public void Add(params string[] files)
        {
            lock (SyncRoot)
            {
                foreach (var fileName in files)
                {
                    items.Add(fileName);
                }
            }
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

        public void Remove(string[] fileNames)
        {
            lock (SyncRoot)
            {
                foreach (var fileName in fileNames)
                {
                    items.Remove(fileName);
                }
            }
        }
    }
}
