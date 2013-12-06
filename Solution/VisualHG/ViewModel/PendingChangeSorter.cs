using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace VisualHg.ViewModel
{
    public class PendingChangeSorter
    {
        private SortDescriptionCollection sortDescriptions;

        private GridViewColumnHeader sortingColumnHeader;
        private ListSortDirection direction;


        public PendingChangeSorter(ListView listView)
        {
            sortDescriptions = listView.Items.SortDescriptions;
        }


        public void SortBy(GridViewColumnHeader columnHeader)
        {
            if (columnHeader.Column == null)
            {
                return;
            }

            var propertyName = GetBindingPropertyName(columnHeader);
            
            if (sortingColumnHeader != null)
            {
                sortDescriptions.Clear();
                AttachedProperty.SetSortDirection(sortingColumnHeader, null);
            }

            if (sortingColumnHeader != columnHeader && !String.IsNullOrEmpty(propertyName))
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