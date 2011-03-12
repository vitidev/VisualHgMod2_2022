using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;
using System.Runtime.Serialization.Formatters.Binary;
using MsVsShell = Microsoft.VisualStudio.Shell;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace VisualHG
{
  public enum VisualHGToolWindow
  {
    None = 0,
    PendingChanges,
  }

  // Register the VisualHG tool window visible only when the provider is active
  [MsVsShell.ProvideToolWindow(typeof(HGPendingChangesToolWindow))]
  [MsVsShell.ProvideToolWindowVisibility(typeof(HGPendingChangesToolWindow), GuidList.ProviderGuid)]
  public partial class SccProvider 
  {
    //public ToolWindowPane FindToolWindow(Type toolWindowType, int id, bool create);
    public void ShowToolWindow(VisualHGToolWindow window)
    {
      ShowToolWindow(window, 0, true);
    }

    Type GetPaneType(VisualHGToolWindow toolWindow)
    {
      switch (toolWindow)
      {
        case VisualHGToolWindow.PendingChanges:
          return typeof(HGPendingChangesToolWindow);
        default:
          throw new ArgumentOutOfRangeException("toolWindow");
      }
    }

    public ToolWindowPane FindToolWindow(VisualHGToolWindow toolWindow)
    {
      ToolWindowPane pane = FindToolWindow(GetPaneType(toolWindow), 0, false);
      return pane;
    }
    
    public void ShowToolWindow(VisualHGToolWindow toolWindow, int id, bool create)
    {
        try
        {
            ToolWindowPane pane = FindToolWindow(GetPaneType(toolWindow), id, create);

            IVsWindowFrame frame = pane.Frame as IVsWindowFrame;
            if (frame == null)
            {
                throw new InvalidOperationException("FindToolWindow failed");
            }
            // Bring the tool window to the front and give it focus
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(frame.Show());
        }
        catch(Exception e)
        {
            MessageBox.Show(e.Message, "Error occured");
        }
    }
  }
}
