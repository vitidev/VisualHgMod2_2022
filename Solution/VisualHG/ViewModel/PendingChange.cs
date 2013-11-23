using HgLib;

namespace VisualHg.ViewModel
{
    public class PendingChange
    {
        public string ShortName { get; set; }
        
        public string RootName { get; set; }

        public ComparableStatus Status { get; set; }

        public ComparablePath Name { get; set; }

        public ComparablePath FullName { get; set; }


        public PendingChange() { }

        public PendingChange(HgFileInfo file)
        {
            ShortName = file.ShortName;
            RootName = file.RootName;
            Status = file.Status;
            Name = file.Name;
            FullName = file.FullName;
        }
    }
}
