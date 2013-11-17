using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace HgLib
{
    public class HgFileInfoDictionary
    {
        private Dictionary<string, HgFileInfo> _files;

        public int Count
        {
            get { return _files.Count; }
        }

        public void Clear()
        {
            _files.Clear();
        }

        public HgFileInfoDictionary()
	    {
            _files = new Dictionary<string, HgFileInfo>();
	    }


        public void Add(HgFileInfo[] files)
        {
            foreach (var file in files)
            {
                SetAt(file.FullName, file);
            }
        }

        public void Remove(string file)
        {
            _files.Remove(file.ToLower());
        }

        void SetAt(string file, HgFileInfo info)
        {
            _files[file.ToLower()] = info;
        }

        public bool TryGetValue(string fileName, out HgFileInfo info)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                info = null;
                return false;
            }

            return _files.TryGetValue(fileName.ToLower(), out info);
        }

        public HgFileInfo[] GetPendingFiles()
        {
            return _files.Values
                .Where(x => (x.Status & HgFileStatus.Pending) > 0)
                .ToArray();
        }

        public bool FileMoved(string fileName, out string newName)
        {
            var root = HgProvider.FindRepositoryRoot(fileName);
            var name = Path.GetFileName(fileName);

            foreach (var fileInfo in _files.Values.Where(x => x.Status == HgFileStatus.Added))
            {
                if (name.Equals(fileInfo.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    var root2 = HgProvider.FindRepositoryRoot(fileInfo.FullName);

                    if (root.Equals(root2, StringComparison.CurrentCultureIgnoreCase))
                    {
                        newName = fileInfo.FullName;
                        return true;
                    }
                }
            }

            newName = "";
            return false;
        }
    }
}