using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using HgLib;

namespace VisualHg.ViewModel
{
    public class PendingChanges : ObservableCollection<PendingChange>
    {
        public object SyncRoot { get; private set; }


        public PendingChanges()
        {
            SyncRoot = new object();
        }


        public void Synchronize(HgFileInfo[] files)
        {
            var collectionChanged = false;
            
            lock (SyncRoot)
            {
                collectionChanged |= RemoveOutdated(files);
                collectionChanged |= AddOrUpdate(files);
            }

            if (collectionChanged)
            {
                NotifyCollectionChanged();
            }
        }

        private bool RemoveOutdated(HgFileInfo[] files)
        {
            var outdated = GetOutdated(files);
            var collectionChanged = outdated.Length > 0;

            foreach (var pendingChange in outdated)
            {
                Items.Remove(pendingChange);
            }

            return collectionChanged;
        }

        private PendingChange[] GetOutdated(HgFileInfo[] files)
        {
            return Items.Where(x => !files.Any(f => x.Equals(f))).ToArray();
        }

        private bool AddOrUpdate(HgFileInfo[] files)
        {
            var collectionChanged = false;

            foreach (var file in files)
            {
                var pendingChange = Items.FirstOrDefault(x => x.Equals(file));

                if (pendingChange == null)
                {
                    Items.Add(new PendingChange(file));
                    collectionChanged = true;
                }
                else
                {
                    pendingChange.Status = file.Status;
                }
            }

            return collectionChanged;
        }

        private void NotifyCollectionChanged()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}