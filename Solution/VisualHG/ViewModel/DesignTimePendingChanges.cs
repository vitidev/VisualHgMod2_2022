using HgLib;

namespace VisualHg.ViewModel
{
    public class DesignTimePendingChangesViewMode : PendingChanges
    {
        public DesignTimePendingChangesViewMode()
        {
            Add(new PendingChange { ShortName = "Modified.cs", Status = HgFileStatus.Modified, RootName = "visualhg2", Name = "Modified.cs" });
            Add(new PendingChange { ShortName = "Renamed.cs", Status = HgFileStatus.Renamed, RootName = "visualhg2", Name = "Renamed.cs" });
            Add(new PendingChange { ShortName = "Added.cs", Status = HgFileStatus.Added, RootName = "visualhg2", Name = "Added.cs" });
        }
    }
}
