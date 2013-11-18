using System;
using System.Collections.Generic;
using System.Linq;

namespace HgLib
{
    public class HgFileInfoDictionary
    {
        private Dictionary<string, HgFileInfo> _files;

        public object SyncRoot { get; private set; }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return _files.Count;
                }
            }
        }

        public HgFileInfo[] PendingFiles
        {
            get
            {
                lock (SyncRoot)
                {
                    return _files.Values
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
                    _files.TryGetValue(fileName, out fileInfo);
                }

                return fileInfo;
            }
        }


        public HgFileInfoDictionary()
	    {
            SyncRoot = new object();
            _files = new Dictionary<string, HgFileInfo>(StringComparer.InvariantCultureIgnoreCase);
	    }


        public void Add(HgFileInfo[] files)
        {
            lock (SyncRoot)
            {
                foreach (var file in files)
                {
                    _files[file.FullName] = file;
                }
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                _files.Clear();
            }
        }

        public void Remove(string fileName)
        {
            lock (SyncRoot)
            {
                _files.Remove(fileName);
            }
        }
    }
}