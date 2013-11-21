using System.Collections.ObjectModel;
using HgLib;

namespace VisualHg.ViewModel
{
    public class PendingChanges : ObservableCollection<PendingChange>
    {
        protected PendingChanges() { }

        public PendingChanges(HgFileInfo[] files)
        {
            foreach (var file in files)
            {
                Add(new PendingChange(file));
            }
        }
    }
}
