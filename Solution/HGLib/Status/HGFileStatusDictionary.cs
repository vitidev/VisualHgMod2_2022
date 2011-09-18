using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HGLib
{
    // ---------------------------------------------------------------------------
    // file status container - maps file name to HGFileStatusInfo object
    // ---------------------------------------------------------------------------
    public class HGFileStatusInfoDictionary
    {
        Dictionary<string, HGFileStatusInfo> _dictionary = new Dictionary<string, HGFileStatusInfo>();

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
                    SetAt(k.Key, new HGFileStatusInfo(k.Value, k.Key));
                }
            }
        }

        public void Remove(string file)
        {
            _dictionary.Remove(file.ToLower());
        }

        void SetAt(string file, HGFileStatusInfo info)
        {
            //Trace.WriteLine("status:" + info.state + " " + file);
            _dictionary[file.ToLower()] = info;
        }

        public bool TryGetValue(string file, out HGFileStatusInfo info)
        {
            if (file != null)
                return _dictionary.TryGetValue(file.ToLower(), out info);

            info = null;
            return false;
        }

        // ------------------------------------------------------------------------
        // Create pending files list
        // ------------------------------------------------------------------------
        public void CreatePendingFilesList(out List<HGFileStatusInfo> list)
        {
          list = new List<HGFileStatusInfo>();
          foreach(HGFileStatusInfo value in _dictionary.Values)
          {
              if (  value.fullPath != null &&
                    value.status != HGLib.HGFileStatus.scsClean &&
                    value.status != HGLib.HGFileStatus.scsIgnored  &&
                    value.status != HGLib.HGFileStatus.scsUncontrolled )
                list.Add(value);
          } 
        }
    }
}
