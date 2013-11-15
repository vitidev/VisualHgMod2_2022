using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VisualHg
{
    public class ListViewColumnIcons
    {
        private const int HDI_FORMAT = 0x0004;
        private const int HDF_LEFT = 0x0000;
        private const int HDF_STRING = 0x4000;
        private const int HDF_SORTUP = 0x0400;
        private const int HDF_SORTDOWN = 0x0200;
        private const int LVM_GETHEADER = 0x1000 + 31;  // LVM_FIRST + 31
        private const int HDM_GETITEM = 0x1200 + 11;  // HDM_FIRST + 11
        private const int HDM_SETITEM = 0x1200 + 12;  // HDM_FIRST + 12
        
        public static void Update(ListView listView, int currentColumnToSort, int newColumnToSort)
        {
            var hHeader = SendMessage(listView.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            var newColumn = new IntPtr(newColumnToSort);
            var currentColumn = new IntPtr(currentColumnToSort);
            
            HDITEM hdItem;
            IntPtr rtn;

            if (currentColumnToSort != -1 && currentColumnToSort != newColumnToSort)
            {
                hdItem = new HDITEM();
                hdItem.mask = HDI_FORMAT;
                rtn = SendMessageITEM(hHeader, HDM_GETITEM, currentColumn, ref hdItem);
            
                hdItem.fmt &= ~HDF_SORTDOWN & ~HDF_SORTUP;
                rtn = SendMessageITEM(hHeader, HDM_SETITEM, currentColumn, ref hdItem);
            }

            hdItem = new HDITEM();
            hdItem.mask = HDI_FORMAT;
            SendMessageITEM(hHeader, HDM_GETITEM, newColumn, ref hdItem);

            if (listView.Sorting == SortOrder.Ascending)
            {
                hdItem.fmt &= ~HDF_SORTDOWN;
                hdItem.fmt |= HDF_SORTUP;
            }
            else if (listView.Sorting == SortOrder.Descending)
            {
                hdItem.fmt &= ~HDF_SORTUP;
                hdItem.fmt |= HDF_SORTDOWN;
            }
            else
            {
                hdItem.fmt &= ~HDF_SORTDOWN;
                hdItem.fmt &= ~HDF_SORTUP;
            }
            
            SendMessageITEM(hHeader, HDM_SETITEM, newColumn, ref hdItem);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessageITEM(IntPtr Handle, int msg, IntPtr wParam, ref HDITEM lParam);


        [StructLayout(LayoutKind.Sequential)]
        private struct HDITEM
        {
            public int mask;
            public int cxy;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public int fmt;
            public int lParam;
            public int iImage;
            public int iOrder;
        };
    }
}
