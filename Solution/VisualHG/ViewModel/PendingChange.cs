using HgLib;

namespace VisualHg.ViewModel
{
    public class PendingChange
    {
        public string ShortName { get; set; }

        public HgFileStatus Status { get; set; }

        public string RootName { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }


        public PendingChange() { }

        public PendingChange(HgFileInfo file)
        {
            ShortName = file.ShortName;
            Status = file.Status;
            RootName = file.RootName;
            Name = file.Name;
            FullName = file.FullName;
        }
    }
}
