using System;
using System.Collections.Generic;
using System.Linq;

namespace HgLib
{
    internal class HgFileInfoDictionary
    {
        private Dictionary<string, HgFileInfo> items;

        public object SyncRoot { get; private set; }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return items.Count;
                }
            }
        }

        public HgFileInfo[] PendingFiles
        {
            get
            {
                lock (SyncRoot)
                {
                    return items.Values
                        .Where(x => x.StatusMatches(HgFileStatus.Pending))
                        .ToArray();
                }
            }
        }

        public HgFileInfo this[string fileName]
        {
            get
            {
                HgFileInfo fileInfo = null;

                lock (SyncRoot)
                {
                    items.TryGetValue(fileName, out fileInfo);
                }

                return fileInfo;
            }
        }


        public HgFileInfoDictionary()
	    {
            SyncRoot = new object();
            items = new Dictionary<string, HgFileInfo>(StringComparer.InvariantCultureIgnoreCase);
	    }


        public void Add(HgFileInfo[] files)
        {
            lock (SyncRoot)
            {
                foreach (var file in files)
                {
                    items[file.FullName] = file;
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

        public void Remove(IEnumerable<string> fileNames)
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