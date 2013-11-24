using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HgLib;
using Microsoft.VisualStudio.Shell;
using VisualHg.Controls;

namespace VisualHg
{
    [Guid(Guids.ToolWindow)]
    public sealed class PendingChangesToolWindow : ToolWindowPane
    {
        private PendingChangesView pendingChangesControl;

        public PendingChangesToolWindow()
        {
            pendingChangesControl = new PendingChangesView();

            Content = pendingChangesControl;
            Caption = Resources.ResourceManager.GetString("100");
        }

        public void Synchronize(HgFileInfo[] files)
        {
            pendingChangesControl.Synchronize(files);
        }
    }
}