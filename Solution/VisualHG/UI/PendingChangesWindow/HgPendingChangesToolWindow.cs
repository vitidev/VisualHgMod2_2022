using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

//using IServiceProvider = System.IServiceProvider;
//using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace VisualHg
{
    /// <summary>
    /// Summary description for SccProviderToolWindow.
    /// </summary>
    [Guid(Guids.HgPendingChangesToolWindowGuid)]
    public class HgPendingChangesToolWindow : ToolWindowPane
    {
        private HgPendingChangesToolWindowControl control;

        public HgPendingChangesToolWindow() :base(null)
        {
            // set the window title
            this.Caption = Resources.ResourceManager.GetString("HgPendingChangesToolWindowCaption");

            // set the CommandID for the window ToolBar
            //this.ToolBar = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.imnuToolWindowToolbarMenu);

            control = new HgPendingChangesToolWindowControl();

            // update pending list
            SccProviderService service = (SccProviderService)SccProvider.GetServiceEx(typeof(SccProviderService));
            if(service!=null)
            {
                UpdatePendingList(service.StatusTracker);
            }
        }

        // route update pending changes call
        public void UpdatePendingList(HgRepositoryTracker tracker)
        {
          control.UpdatePendingList(tracker);
        }

        // ------------------------------------------------------------------------
        // returns the window handle
        // ------------------------------------------------------------------------
        override public IWin32Window Window
        {
            get
            {
                return (IWin32Window)control;
            }
        }

        /// <include file='doc\WindowPane.uex' path='docs/doc[@for="WindowPane.Dispose1"]' />
        /// <devdoc>
        ///     Called when this tool window pane is being disposed.
        /// </devdoc>
        override protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (control != null)
                {
                    try
                    {
                        if (control is IDisposable)
                            control.Dispose();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.Fail(String.Format("Failed to dispose {0} controls.\n{1}", this.GetType().FullName, e.Message));
                    }
                    control = null;
                } 
                
                IVsWindowFrame windowFrame = (IVsWindowFrame)this.Frame;
                if (windowFrame != null)
                {
                    // Note: don't check for the return code here.
                    windowFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_SaveIfDirty);
                }
            }
            base.Dispose(disposing);
        }
    }
}
