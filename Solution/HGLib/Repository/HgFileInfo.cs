using System;
using System.IO;

namespace HgLib
{
    public class HgFileInfo
    {
        private long length;
        private DateTime lastWriteTime;
        private HgFileInfo _originalFile;


        internal HgFileInfo OriginalFile
        {
            get { return _originalFile; }
            set
            {
                _originalFile = value;

                if (_originalFile != null)
                {
                    Status = File.Exists(_originalFile.FullName) ? HgFileStatus.Copied : HgFileStatus.Renamed;
                }
            }
        }


        public string Name { get; private set; }

        public string FullName { get; private set; }

        public HgFileStatus Status { get; private set; }

        public string OriginalName
        {
            get { return _originalFile != null ? _originalFile.Name : Name; }
        }

        public string OriginalFullName
        {
            get { return _originalFile != null ? _originalFile.FullName : FullName; }
        }
        
        public bool HasChanged
        {
            get
            {
                if (Status == HgFileStatus.Removed || Status == HgFileStatus.NotTracked)
                {
                    return false;
                }

                var file = new FileInfo(FullName);

                return !file.Exists || file.Length != length || file.LastWriteTime != lastWriteTime;
            }
        }


        public HgFileInfo(string fileName, char status)
        {
            var file = new FileInfo(fileName);
            
            FullName = fileName;
            Name = Path.GetFileName(fileName);
            Status = Hg.GetStatus(status);

            if (file.Exists)
            {
                length = file.Length;
                lastWriteTime = file.LastWriteTime;
            }
        }


        public bool StatusMatches(HgFileStatus status)
        {
            return (Status & status) > 0;
        }
    }
}