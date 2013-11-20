using System;
using System.Runtime.InteropServices;

namespace VisualHg
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        internal static extern IntPtr SendMessageITEM(IntPtr hWnd, int msg, IntPtr wParam, ref HDITEM lParam);
        
        [DllImport("user32.dll")]
        internal static extern bool SetWindowText(IntPtr hWnd, string lpString);


        [StructLayout(LayoutKind.Sequential)]
        internal struct HDITEM
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
