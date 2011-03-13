using System;
using System.IO;

namespace HGLib
{
    // ---------------------------------------------------------------------------
    // file status info object for one file including state, date and size entries
    // ---------------------------------------------------------------------------
    public class HGFileStatusInfo
    {
        // last write time stamp
        public DateTime timeStamp;
        // hg state of file
        public char state;
        // file size
        public long size;
        // case sensitive full path
        public string fullPath;
        // case sensitive file name
        public string fileName;

        // init the file props by getting the size and time via FileInfo object
        public HGFileStatusInfo(char state, string path)
        {
            this.state = state;

            if (state == 'R')
            {
              timeStamp = DateTime.Now;
              size = 0;
              fullPath = path;

              int i = path.LastIndexOf('\\');
              if (i > 0)
                fileName = path.Substring(i + 1);
            }
            else
            {
              FileInfo fileInfo = new FileInfo(path);
              if (fileInfo.Exists)
              {
                  timeStamp = fileInfo.LastWriteTime;
                  size = fileInfo.Length;
                  fullPath = fileInfo.FullName;
                  fileName = fileInfo.Name;
              }
           }   
        }
    }
}
