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
        public string caseSensitiveFileName;

        // init the file props by getting the size and time via FileInfo object
        public HGFileStatusInfo(char state, string fileName)
        {
            this.state = state;

            FileInfo fileInfo = new FileInfo(fileName);
            if (fileInfo.Exists)
            {
                timeStamp = fileInfo.LastWriteTime;
                size = fileInfo.Length;
                caseSensitiveFileName = fileName;
            }
        }
        
        public string FileName()
        {
            if (caseSensitiveFileName != null)
            {
                int i = caseSensitiveFileName.LastIndexOf('\\');
                if (i > 0)
                    return caseSensitiveFileName.Substring(i + 1);

                return caseSensitiveFileName;
            }
            return "";
        }
    }
}
