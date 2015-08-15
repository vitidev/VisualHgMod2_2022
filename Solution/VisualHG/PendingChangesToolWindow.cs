using System;
using System.Runtime.InteropServices;
using HgLib;
using Microsoft.VisualStudio.Shell;
using VisualHg.Controls;

namespace VisualHg
{
    [Guid(Guids.ToolWindow)]
    public sealed class PendingChangesToolWindow : ToolWindowPane
    {
        private PendingChangesView pendingChangesControl;
        public event EventHandler<EventArgs> NeedsRefresh;

        public PendingChangesToolWindow()
        {
            pendingChangesControl = new PendingChangesView();
            pendingChangesControl.NeedsRefresh += PendingChangesControl_NeedsRefresh;
            Content = pendingChangesControl;
            Caption = Resources.ResourceManager.GetString("100");
        }

        private void PendingChangesControl_NeedsRefresh(object sender, EventArgs e)
        {
            NeedsRefresh?.Invoke(this, EventArgs.Empty);
        }

        public void Synchronize(HgFileInfo[] files)
        {
            pendingChangesControl.Synchronize(files);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                pendingChangesControl.NeedsRefresh -= PendingChangesControl_NeedsRefresh;
            }
        }
    }
}