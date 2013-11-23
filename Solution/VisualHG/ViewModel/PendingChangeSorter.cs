using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace VisualHg.ViewModel
{
    public class PendingChangeSorter : IComparer
    {
        private SortDescriptionCollection sortDescriptions;

        private GridViewColumnHeader sortingColumnHeader;
        private ListSortDirection direction;


        public PendingChangeSorter(ListView listView)
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

        public void SortBy(GridViewColumnHeader columnHeader)
        {
            var propertyName = GetBindingPropertyName(columnHeader);
            
            if (sortingColumnHeader != null)
            {
                sortDescriptions.Clear();
                AttachedProperty.SetSortDirection(sortingColumnHeader, null);
            }

            if (sortingColumnHeader != columnHeader)
            {
                direction = ListSortDirection.Ascending;
                sortingColumnHeader = columnHeader;
            }
            else if (direction == ListSortDirection.Ascending)
            {
                direction = ListSortDirection.Descending;
            }
            else
            {
                sortingColumnHeader = null;
            }

            if (sortingColumnHeader != null)
            {
                sortDescriptions.Add(new SortDescription(propertyName, direction));
                AttachedProperty.SetSortDirection(sortingColumnHeader, direction);
            }
        }


        public int Compare(object x, object y)
        {
            return Compare((PendingChange)x, (PendingChange)y);
        }

        public int Compare(PendingChange x, PendingChange y)
        {
            var result = x.Status.CompareTo(y.Status);

            if (result == 0)
            {
                result = x.Name.CompareTo(y.Name);
            }

            return result;
        }

        
        private static string GetBindingPropertyName(GridViewColumnHeader columnHeader)
        {
            if (columnHeader == null || columnHeader.Column == null)
            {
                return null;
            }

            var binding = columnHeader.Column.DisplayMemberBinding as Binding;

            if (binding != null && binding.Path != null)
            {
                return binding.Path.Path;
            }

            return "ShortName";
        }
    }
}