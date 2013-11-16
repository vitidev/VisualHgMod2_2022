using System;
using System.IO;

namespace HgLib
{
    public class HgFileInfo
    {
        private long length;
        private DateTime lastWriteTime;

        public string Name { get; private set; }

        public string FullName { get; private set; }

        public HgFileStatus Status { get; private set; }
        
        public bool HasChanged
        {
            get
            {
                if (Status == HgFileStatus.Removed || Status == HgFileStatus.Uncontrolled)
                {
                    return false;
                }

                var file = new FileInfo(FullName);

                return !file.Exists || file.Length != length || file.LastWriteTime != lastWriteTime;
            }
        }


        public HgFileInfo(char status, string path)
        {
            var file = new FileInfo(path);
            
            FullName = path;
            Name = Path.GetFileName(path);
            Status = Hg.GetStatus(status);

            if (file.Exists)
            {
                length = file.Length;
                lastWriteTime = file.LastWriteTime;
            }
        }
    }
}
