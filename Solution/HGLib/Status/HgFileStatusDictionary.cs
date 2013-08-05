using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace HgLib
{
    // ---------------------------------------------------------------------------
    // file status container - maps file name to HgFileStatusInfo object
    // ---------------------------------------------------------------------------
    public class HgFileStatusInfoDictionary
    {
        Dictionary<string, HgFileStatusInfo> _dictionary = new Dictionary<string, HgFileStatusInfo>();

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public void Add(Dictionary<string, char> newFiles)
        {
            Dictionary<string, bool> addedFiles = new Dictionary<string, bool>();

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
                    SetAt(k.Key, new HgFileStatusInfo(k.Value, k.Key));
                }
            }
        }

        public void Remove(string file)
        {
            _dictionary.Remove(file.ToLower());
        }

        void SetAt(string file, HgFileStatusInfo info)
        {
            //Trace.WriteLine("status:" + info.state + " " + file);
            _dictionary[file.ToLower()] = info;
        }

        public bool TryGetValue(string file, out HgFileStatusInfo info)
        {
            if (file != null)
                return _dictionary.TryGetValue(file.ToLower(), out info);

            info = null;
            return false;
        }

        // ------------------------------------------------------------------------
        // Create pending files list
        // ------------------------------------------------------------------------
        public void CreatePendingFilesList(out List<HgFileStatusInfo> list)
        {
          list = new List<HgFileStatusInfo>();
          foreach(HgFileStatusInfo value in _dictionary.Values)
          {
              if (  value.fullPath != null &&
                    value.status != HgLib.HgFileStatus.scsClean &&
                    value.status != HgLib.HgFileStatus.scsIgnored  &&
                    value.status != HgLib.HgFileStatus.scsUncontrolled )
                list.Add(value);
          } 
        }

        // ------------------------------------------------------------------------
        // detect moved files
        // ------------------------------------------------------------------------
        public bool FileMoved(string fileName, out string newName)
        {
            string root = HgProvider.FindRepositoryRoot(fileName);
            string name = Path.GetFileName(fileName);
            foreach (HgFileStatusInfo value in _dictionary.Values)
            {
                if (value.status == HgLib.HgFileStatus.scsAdded)
                {
                    if (name.Equals(value.fileName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        string root2 = HgProvider.FindRepositoryRoot(value.fullPath);
                        if (root.Equals(root2, StringComparison.CurrentCultureIgnoreCase))
                        {
                            newName = value.fullPath;
                            return true;
                        }
                    }
                }
            } 
            newName =""; 
            return false;
        }
    }
}
