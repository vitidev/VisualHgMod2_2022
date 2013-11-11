using System;
using System.Collections.Generic;
using System.IO;

namespace HgLib
{
    public class HgFileInfoDictionary
    {
        Dictionary<string, HgFileInfo> _fileInfo = new Dictionary<string, HgFileInfo>();

        public int Count
        {
            get { return _fileInfo.Count; }
        }

        public void Clear()
        {
            _fileInfo.Clear();
        }

        public void Add(Dictionary<string, char> newFiles)
        {
            var addedFiles = new Dictionary<string, bool>();

            foreach (var k in newFiles)
            {
                // detect case type dependend renames e.g. resource.h to Resource.h
                if (k.Value == 'A')
                {
                    addedFiles[k.Key.ToLower()] = true;
                }

                if (k.Value == 'R' && addedFiles.ContainsKey(k.Key.ToLower()))
                {
                    // don't add this removed status because it comes from remove add sequence 
                    // the file was renamed, so we skip this file status
                }
                else
                {
                    SetAt(k.Key, new HgFileInfo(k.Value, k.Key));
                }
            }
        }

        public void Remove(string file)
        {
            _fileInfo.Remove(file.ToLower());
        }

        void SetAt(string file, HgFileInfo info)
        {
            _fileInfo[file.ToLower()] = info;
        }

        public bool TryGetValue(string file, out HgFileInfo info)
        {
            if (file == null)
            {
                info = null;
                return false;
            }

            return _fileInfo.TryGetValue(file.ToLower(), out info);
        }

        public List<HgFileInfo> GetPendingFiles()
        {
            var pending = new List<HgFileInfo>();
            
            foreach (var fileInfo in _fileInfo.Values)
            {
                if (fileInfo.FullName != null &&
                      fileInfo.Status != HgFileStatus.Clean &&
                      fileInfo.Status != HgFileStatus.Ignored &&
                      fileInfo.Status != HgFileStatus.Uncontrolled)
                    pending.Add(fileInfo);
            }

            return pending;
        }

        public bool FileMoved(string fileName, out string newName)
        {
            var root = HgProvider.FindRepositoryRoot(fileName);
            var name = Path.GetFileName(fileName);
            
            foreach (var fileInfo in _fileInfo.Values)
            {
                if (fileInfo.Status == HgLib.HgFileStatus.Added)
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
            }

            newName = "";
            return false;
        }
    }
}
