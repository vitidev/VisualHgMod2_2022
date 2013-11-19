using System;
using System.IO;

namespace HgLib
{
    public class HgFileInfo
    {
        private bool exists;
        private DateTime lastWriteTime;
        private HgFileStatus _status;


        internal HgFileInfo OriginalFile { get; set; }

        public string Root { get; private set; }

        public string RootName { get; private set; }

        public string Name { get; private set; }
        
        public string ShortName { get; private set; }

        public string FullName { get; private set; }

        public HgFileStatus Status
        {
            get
            {
                if (OriginalFile != null)
                {
                    return OriginalFile.exists ? HgFileStatus.Copied : HgFileStatus.Renamed;
                }

                return _status;
            }
        }

        public string OriginalName
        {
            get { return OriginalFile != null ? OriginalFile.Name : Name; }
        }

        public string OriginalFullName
        {
            get { return OriginalFile != null ? OriginalFile.FullName : FullName; }
        }

        public bool HasChanged
        {
            get
            {
                if (StatusMatches(HgFileStatus.NotAdded))
                {
                    return false;
                }

                try
                {
                    var file = new FileInfo(FullName);

                    return exists != file.Exists || file.LastWriteTime != lastWriteTime;
                }
                catch
                {
                    return false;
                }
            }
        }

        
        public HgFileInfo(string root, string name, char status)
        {
            Root = root;
            Name = name;
            _status = Hg.ConvertToStatus(status);
            RootName = new DirectoryInfo(root).Name;
            ShortName = Path.GetFileName(name);
            FullName = Path.Combine(root, name);

            if (Status != HgFileStatus.None && !StatusMatches(HgFileStatus.Deleted))
            {
                InitializeFileProperties(FullName);
            }
        }

        private void InitializeFileProperties(string fileName)
        {
            try
            {
                var file = new FileInfo(fileName);

                if (file.Exists)
                {
                    exists = true;
                    lastWriteTime = file.LastWriteTime;
                }
            }
            catch { }
        }


        public bool StatusMatches(HgFileStatus pattern)
        {
            return Status == pattern || (Status & pattern) > 0;
        }

        public static HgFileInfo FromHgOutput(string root, string output)
        {
            return new HgFileInfo(root, output.Substring(2), output[0]);
        }

        public override string ToString()
        {
            return String.Concat(Status, ' ', Name);
        }
    }
}