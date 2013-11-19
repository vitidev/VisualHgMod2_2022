using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using HgLib;

namespace VisualHg
{
    public class HgFileInfoComparer : IComparer<HgFileInfo>
    {   
        public int ColumnToSort { get; private set; }

        public SortOrder Sorting { get; private set; }


        public HgFileInfoComparer()
        {
            ColumnToSort = -1;
            Sorting = SortOrder.None;
        }


        public void SortByColumn(int columnIndex)
        {
            if (ColumnToSort != columnIndex || Sorting == SortOrder.None)
            {
                Sorting = SortOrder.Ascending;
            }
            else if (Sorting == SortOrder.Ascending)
            {
                Sorting = SortOrder.Descending;
            }
            else
            {
                Sorting = SortOrder.None;
            }

            ColumnToSort = columnIndex;
        }

        public int Compare(HgFileInfo x, HgFileInfo y)
        {
            if (ColumnToSort < 0 || Sorting == SortOrder.None)
            {
                return CompareDefault(x, y);
            }

            return Compare(x, y, ColumnToSort);
        }

        private int CompareDefault(HgFileInfo x, HgFileInfo y)
        {
            var result = Compare(x, y, HgFileInfoListViewItem.StatusColumn);

            if (result == 0)
            {
                result = Compare(x, y, HgFileInfoListViewItem.PathColumn);
            }

            return result;
        }

        private int Compare(HgFileInfo x, HgFileInfo y, int columnToSort)
        {
            var xText = HgFileInfoListViewItem.GetText(x, columnToSort);
            var yText = HgFileInfoListViewItem.GetText(y, columnToSort);

            var result = 0;

            if (columnToSort == HgFileInfoListViewItem.PathColumn)
            {
                result = GetPathDepth(xText).CompareTo(GetPathDepth(yText));
            }

            if (result == 0)
            {
                result = CaseInsensitiveComparer.DefaultInvariant.Compare(xText, yText);
            }

            if (Sorting == SortOrder.Descending)
            {
                return -result;
            }

            return result;
        }

        private static int GetPathDepth(string path)
        {
            return path.Count(x => x == '\\');
        }
    }
}
