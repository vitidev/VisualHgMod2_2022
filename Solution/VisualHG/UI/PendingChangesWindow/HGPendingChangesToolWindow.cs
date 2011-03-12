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

namespace VisualHG
{
    /// <summary>
    /// Summary description for SccProviderToolWindow.
    /// </summary>
    [Guid(GuidList.HGPendingChangesToolWindowGuid)]
    public class HGPendingChangesToolWindow : ToolWindowPane
    {
        private HGPendingChangesToolWindowControl control;

        public HGPendingChangesToolWindow() :base(null)
        {
            // set the window title
            this.Caption = Resources.ResourceManager.GetString("HGPendingChangesToolWindowCaption");

            // set the CommandID for the window ToolBar
            //this.ToolBar = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.imnuToolWindowToolbarMenu);

            // set the icon for the frame
            this.BitmapResourceID = CommandId.ibmpToolWindowsImages;  // bitmap strip resource ID
            this.BitmapIndex = CommandId.iconSccProviderToolWindow;   // index in the bitmap strip

            //IServiceContainer _container = UISite.GetService<IServiceContainer>(); 
            
            control = new HGPendingChangesToolWindowControl();
        }

        // route update pending changes call
        public void UpdatePendingList(HGStatusTracker tracker)
        {
          control.UpdatePendingList(tracker);
        }


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

        /// <summary>
        /// This function is only used to "do something noticeable" when the toolbar button is clicked.
        /// It is called from the package.
        /// A typical tool window may not need this function.
        /// 
        /// The current behavior change the background color of the control
        /// </summary>
        public void ToolWindowToolbarCommand()
        {
            if (this.control.BackColor == Color.Coral)
                this.control.BackColor = Color.White;
            else
                this.control.BackColor = Color.Coral;
        }
    }
}
