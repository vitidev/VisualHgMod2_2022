using System;
using System.IO;

namespace HgLib
{
    public class HgFileInfo
    {
        public DateTime LastWriteTime { get; private set; }

        public long Length { get; private set; }

        public string FullName { get; private set; }

        public string Name { get; private set; }

        public HgFileStatus Status { get; private set; }


        public HgFileInfo(char status, string path)
        {
            switch (status)
            {
                case 'C': Status = HgFileStatus.Clean; break;
                case 'M': Status = HgFileStatus.Modified; break;
                case 'A': Status = HgFileStatus.Added; break;
                case 'R': Status = HgFileStatus.Removed; break;
                case 'I': Status = HgFileStatus.Ignored; break;
                case 'N': Status = HgFileStatus.Renamed; break;
                case 'P': Status = HgFileStatus.Copied; break;
                case '?': Status = HgFileStatus.Uncontrolled; break;
                case '!': Status = HgFileStatus.Missing; break;
            }

            if (Status == HgFileStatus.Removed)
            {
                FullName = path;
                LastWriteTime = DateTime.Now;
                Length = 0;
                Name = Path.GetFileName(path);
            }
            else if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                FullName = fileInfo.FullName;
                LastWriteTime = fileInfo.LastWriteTime;
                Length = fileInfo.Length;
                Name = fileInfo.Name;
            }
        }
    }
}
