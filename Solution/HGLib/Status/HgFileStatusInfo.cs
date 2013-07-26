using System;
using System.IO;

namespace HgLib
{
    // ---------------------------------------------------------------------------
    // file status info object for one file including state, date and size entries
    // ---------------------------------------------------------------------------
    public class HgFileStatusInfo
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
        public HgFileStatus status;

        // init the file props by getting the size and time via FileInfo object
        public HgFileStatusInfo(char statusByte, string path)
        {
            switch (statusByte)
            {
                case 'C': status = HgFileStatus.scsClean; break;
                case 'M': status = HgFileStatus.scsModified; break;
                case 'A': status = HgFileStatus.scsAdded; break;
                case 'R': status = HgFileStatus.scsRemoved; break;
                case 'I': status = HgFileStatus.scsIgnored; break;
                case 'N': status = HgFileStatus.scsRenamed; break;
                case 'P': status = HgFileStatus.scsCopied; break;
                case '?': status = HgFileStatus.scsUncontrolled; break;
                case '!': status = HgFileStatus.scsMissing; break;
            }

            if (HgFileStatus.scsRemoved == status)
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
