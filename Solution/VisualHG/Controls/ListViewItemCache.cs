using System;
using System.Windows.Forms;

namespace VisualHg.Controls
{
    public class ListViewItemCache
    {
        public static readonly ListViewItemCache Empty = new ListViewItemCache(-1, new ListViewItem[0]);

        private ListViewItem[] _items;

        public int StartIndex { get; private set; }

        public int EndIndex { get; private set; }


        public ListViewItemCache(int startIndex, ListViewItem[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            StartIndex = startIndex;
            EndIndex = startIndex + items.Length;

            _items = items;
        }


        public bool Contains(int index)
        {
            return index >= StartIndex && index < EndIndex;
        }

        public bool Contains(int startIndex, int endIndex)
        {
            return startIndex >= StartIndex && endIndex <= EndIndex;
        }

        public ListViewItem GetItem(int index)
        {
            return _items[index - StartIndex];
        }
    }
}
