using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using HgLib;

namespace VisualHg.ViewModel
{
    public class PendingChangeComparer : IComparer
    {
        private SortDescriptionCollection sortDescriptions;
        private Adorner sortingIcons;
   
        public GridViewColumnHeader ColumnToSort { get; private set; }

        public ListSortDirection Direction { get; private set; }


        public PendingChangeComparer(ListView listView)
        {
            sortDescriptions = listView.Items.SortDescriptions;
            listView.DataContextChanged += OnListViewDataContextChanged;
        }

        private void OnListViewDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var listView = (ListView)sender;
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
            
            if (view != null)
            {
                view.CustomSort = this;
            }
        }

        public void SortBy(GridViewColumnHeader column)
        {
            var sortBy = column.Tag.ToString();
            
            if (ColumnToSort != null)
            {
                AdornerLayer.GetAdornerLayer(ColumnToSort).Remove(sortingIcons);
                sortDescriptions.Clear();
            }

            if (ColumnToSort != column)
            {
                Direction = ListSortDirection.Ascending;
                ColumnToSort = column;
            }
            else if (Direction == ListSortDirection.Ascending)
            {
                Direction = ListSortDirection.Descending;
            }
            else
            {
                ColumnToSort = null;
            }

            if (ColumnToSort != null)
            {
                sortingIcons = new ListViewSortingIcons(ColumnToSort, Direction);
                AdornerLayer.GetAdornerLayer(ColumnToSort).Add(sortingIcons);
                sortDescriptions.Add(new SortDescription(sortBy, Direction));
            }
        }

        public int Compare(object x, object y)
        {
            return Compare((PendingChange)x, (PendingChange)y);
        }

        public int Compare(PendingChange x, PendingChange y)
        {
            if (ColumnToSort == null)
            {
                return CompareDefault(x, y);
            }

            return Compare(x, y, ColumnToSort.Tag.ToString());
        }


        private int CompareDefault(PendingChange x, PendingChange y)
        {
            var result = Compare(x, y, "Status");

            if (result == 0)
            {
                result = Compare(x, y, "Name");
            }

            return result;
        }

        private int Compare(PendingChange x, PendingChange y, string propertyName)
        {
            var xText = GetText(x, propertyName);
            var yText = GetText(y, propertyName);

            var result = 0;

            if (propertyName == "Status")
            {
                result = GetStatusPriority(x.Status).CompareTo(GetStatusPriority(y.Status));
            }
            else if (propertyName == "Name")
            {
                result = GetPathDepth(xText).CompareTo(GetPathDepth(yText));
            }

            if (result == 0)
            {
                result = CaseInsensitiveComparer.DefaultInvariant.Compare(xText, yText);
            }

            if (Direction == ListSortDirection.Descending)
            {
                return -result;
            }

            return result;
        }

        private string GetText(PendingChange item, string propertyName)
        {
            switch (propertyName)
            {
                case "Name":
                    return item.Name;
                
                case "ShortName":
                    return item.ShortName;
                
                default:
                    return item.RootName;
            }
        }

        private static int GetPathDepth(string path)
        {
            return path.Count(x => x == '\\');
        }

        private static int GetStatusPriority(HgFileStatus status)
        {
            switch (status)
            {
                case HgFileStatus.Modified:
                    return 0;
                case HgFileStatus.Copied:
                    return 2;
                case HgFileStatus.Renamed:
                    return 1;
                case HgFileStatus.Added:
                    return 3;
                case HgFileStatus.Removed:
                    return 4;
                case HgFileStatus.Missing:
                    return 5;
                case HgFileStatus.NotTracked:
                    return 6;
                case HgFileStatus.Ignored:
                    return 7;
                case HgFileStatus.Clean:
                    return 8;
                default:
                    return 9;
            }
        }
    }
}