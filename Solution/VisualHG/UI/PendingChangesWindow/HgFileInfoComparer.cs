using System;
using System.Collections;
using System.Collections.Generic;
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
            if (Sorting != SortOrder.Ascending || ColumnToSort != columnIndex)
            {
                Sorting = SortOrder.Ascending;
            }
            else if (Sorting == SortOrder.Ascending)
            {
                Sorting = SortOrder.Descending;
            }

            ColumnToSort = columnIndex;
        }

        public int Compare(HgFileInfo x, HgFileInfo y)
        {
            if (ColumnToSort < -1)
            {
                return 0;
            }

            var result = CaseInsensitiveComparer.DefaultInvariant.Compare(GetText(x), GetText(y));

            if (Sorting == SortOrder.Descending)
            {
                return -result;
            }

            return result;
        }

        private string GetText(HgFileInfo fileInfo)
        {
            if (ColumnToSort == 1)
            {
                return fileInfo.FullName;
            }

            return fileInfo.Name;
        }
    }
}
