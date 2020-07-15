using System;
using System.ComponentModel;
using System.Windows;

namespace VisualHg.ViewModel
{
    public static class AttachedProperty
    {
        public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.RegisterAttached
        ("SortDirection",
            typeof(ListSortDirection?),
            typeof(AttachedProperty),
            new FrameworkPropertyMetadata(null));


        public static ListSortDirection? GetSortDirection(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (ListSortDirection?)element.GetValue(SortDirectionProperty);
        }

        public static void SetSortDirection(UIElement element, ListSortDirection? value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(SortDirectionProperty, value);
        }
    }
}