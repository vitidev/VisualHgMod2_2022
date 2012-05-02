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
        // file size
        public long size;
        // case sensitive full path
        public string fullPath;
        // case sensitive file name
        public string fileName;
        // source control file status
        public HGFileStatus status;

        // init the file props by getting the size and time via FileInfo object
        public HGFileStatusInfo(char statusByte, string path)
        {
            switch (statusByte)
            {
                case 'C': status = HGFileStatus.scsClean; break;
                case 'M': status = HGFileStatus.scsModified; break;
                case 'A': status = HGFileStatus.scsAdded; break;
                case 'R': status = HGFileStatus.scsRemoved; break;
                case 'I': status = HGFileStatus.scsIgnored; break;
                case 'N': status = HGFileStatus.scsRenamed; break;
                case 'P': status = HGFileStatus.scsCopied; break;
                case '?': status = HGFileStatus.scsUncontrolled; break;
                case '!': status = HGFileStatus.scsMissing; break;
            }

            if (HGFileStatus.scsRemoved == status)
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
                if (File.Exists(path))
                {
                  FileInfo fileInfo = new FileInfo(path);
                  timeStamp = fileInfo.LastWriteTime;
                  size = fileInfo.Length;
                  fullPath = fileInfo.FullName;
                  fileName = fileInfo.Name;
                }
           }   
        }
    }
}
