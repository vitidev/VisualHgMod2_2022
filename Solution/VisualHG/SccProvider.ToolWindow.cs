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

namespace VisualHg
{
  public enum VisualHgToolWindow
  {
    None = 0,
    PendingChanges,
  }

  // Register the VisualHg tool window visible only when the provider is active
  [MsVsShell.ProvideToolWindow(typeof(HgPendingChangesToolWindow), Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Bottom, Transient = false, Window = ToolWindowGuids80.Outputwindow)]
  [MsVsShell.ProvideToolWindowVisibility(typeof(HgPendingChangesToolWindow), GuidList.ProviderGuid)]
  public partial class SccProvider 
  {
    //public ToolWindowPane FindToolWindow(Type toolWindowType, int id, bool create);
    public void ShowToolWindow(VisualHgToolWindow window)
    {
      ShowToolWindow(window, 0, true);
    }

    Type GetPaneType(VisualHgToolWindow toolWindow)
    {
      switch (toolWindow)
      {
        case VisualHgToolWindow.PendingChanges:
          return typeof(HgPendingChangesToolWindow);
        default:
          throw new ArgumentOutOfRangeException("toolWindow");
      }
    }

    public ToolWindowPane FindToolWindow(VisualHgToolWindow toolWindow)
    {
      ToolWindowPane pane = FindToolWindow(GetPaneType(toolWindow), 0, false);
      return pane;
    }
    
    public void ShowToolWindow(VisualHgToolWindow toolWindow, int id, bool create)
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
