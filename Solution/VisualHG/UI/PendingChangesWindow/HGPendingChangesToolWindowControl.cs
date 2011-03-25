using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.VisualStudio.Shell;

namespace VisualHG
{
  /// <summary>
  /// Summary description for SccProviderToolWindowControl.
  /// </summary>
  public class HGPendingChangesToolWindowControl : System.Windows.Forms.UserControl
  {
    private ColumnHeader columnHeaderStatus;
    private ColumnHeader columnHeaderFileName;
    private IContainer components;
    private ContextMenuStrip pendingChangesContextMenu;
    private ToolStripMenuItem commitToolStripMenuItem;
    private ToolStripMenuItem revertToolStripMenuItem;
    private ToolStripMenuItem annotateFileToolStripMenuItem;
    private ToolStripMenuItem openInEditorToolStripMenuItem;
    private ToolStripMenuItem diffToolStripMenuItem;
    private ToolStripMenuItem historyToolStripMenuItem;
    private PendingItemsListView _pendingItemsListView;

    public HGPendingChangesToolWindowControl()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      ImageList menuImageList = _pendingItemsListView._ImageMapper.MenuImageList;
      if (menuImageList != null)
      { 
          this.commitToolStripMenuItem.Image = menuImageList.Images[0];
          this.diffToolStripMenuItem.Image = menuImageList.Images[4];
          this.revertToolStripMenuItem.Image = menuImageList.Images[8];
          this.historyToolStripMenuItem.Image = menuImageList.Images[9];
          this.annotateFileToolStripMenuItem.Image = menuImageList.Images[7];
          this.openInEditorToolStripMenuItem.Image = menuImageList.Images[1];
      }
    }

    /// <summary> 
    /// Let this control process the mnemonics.
    /// </summary>
    protected override bool ProcessDialogChar(char charCode)
    {
      // If we're the top-level form or control, we need to do the mnemonic handling
      if (charCode != ' ' && ProcessMnemonic(charCode))
      {
        return true;
      }
      return base.ProcessDialogChar(charCode);
    }

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HGPendingChangesToolWindowControl));
        this.pendingChangesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.commitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.diffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.revertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.historyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.annotateFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.openInEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this._pendingItemsListView = new VisualHG.PendingItemsListView();
        this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.pendingChangesContextMenu.SuspendLayout();
        this.SuspendLayout();
        // 
        // pendingChangesContextMenu
        // 
        this.pendingChangesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitToolStripMenuItem,
            this.diffToolStripMenuItem,
            this.revertToolStripMenuItem,
            this.historyToolStripMenuItem,
            this.annotateFileToolStripMenuItem,
            this.openInEditorToolStripMenuItem});
        this.pendingChangesContextMenu.Name = "contextMenuStrip1";
        resources.ApplyResources(this.pendingChangesContextMenu, "pendingChangesContextMenu");
        // 
        // commitToolStripMenuItem
        // 
        this.commitToolStripMenuItem.Name = "commitToolStripMenuItem";
        resources.ApplyResources(this.commitToolStripMenuItem, "commitToolStripMenuItem");
        this.commitToolStripMenuItem.Click += new System.EventHandler(this.OnCommitSelectedFiles);
        // 
        // diffToolStripMenuItem
        // 
        this.diffToolStripMenuItem.Name = "diffToolStripMenuItem";
        resources.ApplyResources(this.diffToolStripMenuItem, "diffToolStripMenuItem");
        this.diffToolStripMenuItem.Click += new System.EventHandler(this.OnDiffSelectedFile);
        // 
        // revertToolStripMenuItem
        // 
        this.revertToolStripMenuItem.Name = "revertToolStripMenuItem";
        resources.ApplyResources(this.revertToolStripMenuItem, "revertToolStripMenuItem");
        this.revertToolStripMenuItem.Click += new System.EventHandler(this.OnRevertSelectedFile);
        // 
        // historyToolStripMenuItem
        // 
        this.historyToolStripMenuItem.Name = "historyToolStripMenuItem";
        resources.ApplyResources(this.historyToolStripMenuItem, "historyToolStripMenuItem");
        this.historyToolStripMenuItem.Click += new System.EventHandler(this.OnShowSelectedFileHistory);
        // 
        // annotateFileToolStripMenuItem
        // 
        this.annotateFileToolStripMenuItem.Name = "annotateFileToolStripMenuItem";
        resources.ApplyResources(this.annotateFileToolStripMenuItem, "annotateFileToolStripMenuItem");
        this.annotateFileToolStripMenuItem.Click += new System.EventHandler(this.OnAnnotateSelectedFile);
        // 
        // openInEditorToolStripMenuItem
        // 
        this.openInEditorToolStripMenuItem.Name = "openInEditorToolStripMenuItem";
        resources.ApplyResources(this.openInEditorToolStripMenuItem, "openInEditorToolStripMenuItem");
        this.openInEditorToolStripMenuItem.Click += new System.EventHandler(this.OnOpenSelectedFiles);
        // 
        // _pendingItemsListView
        // 
        this._pendingItemsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStatus,
            this.columnHeaderFileName});
        this._pendingItemsListView.ContextMenuStrip = this.pendingChangesContextMenu;
        resources.ApplyResources(this._pendingItemsListView, "_pendingItemsListView");
        this._pendingItemsListView.FullRowSelect = true;
        this._pendingItemsListView.GridLines = true;
        this._pendingItemsListView.HideSelection = false;
        this._pendingItemsListView.Name = "_pendingItemsListView";
        this._pendingItemsListView.ShowGroups = false;
        this._pendingItemsListView.UseCompatibleStateImageBehavior = false;
        this._pendingItemsListView.View = System.Windows.Forms.View.Details;
        this._pendingItemsListView.VirtualMode = true;
        this._pendingItemsListView.SelectedIndexChanged += new System.EventHandler(this._pendingItemsListView_SelectedIndexChanged);
        this._pendingItemsListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this._pendingItemsListView_KeyDown);
        this._pendingItemsListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this._pendingItemsListView_MouseDoubleClick);
        this._pendingItemsListView.Resize += new System.EventHandler(this._pendingItemsListView_Resize);
        // 
        // columnHeaderStatus
        // 
        resources.ApplyResources(this.columnHeaderStatus, "columnHeaderStatus");
        // 
        // columnHeaderFileName
        // 
        resources.ApplyResources(this.columnHeaderFileName, "columnHeaderFileName");
        // 
        // HGPendingChangesToolWindowControl
        // 
        this.BackColor = System.Drawing.SystemColors.Window;
        this.Controls.Add(this._pendingItemsListView);
        this.Name = "HGPendingChangesToolWindowControl";
        resources.ApplyResources(this, "$this");
        this.pendingChangesContextMenu.ResumeLayout(false);
        this.ResumeLayout(false);

    }
    #endregion

    // ------------------------------------------------------------------------
    // update pending list with status tracker
    // ------------------------------------------------------------------------
    public void UpdatePendingList(HGStatusTracker tracker)
    {
      _pendingItemsListView.UpdatePendingList(tracker);
    }

    // ------------------------------------------------------------------------
    // open selected files in the editor 
    // ------------------------------------------------------------------------
    void OpenSelectedFiles()
    {
      foreach (int index in _pendingItemsListView.SelectedIndices)
      {
        HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
        try
        {
          VsShellUtilities.OpenDocument(SccProvider.Provider, info.fullPath);
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Open File failed");
        }
      }
    }

    // ------------------------------------------------------------------------
    // open selected files on mouse dbl click
    // ------------------------------------------------------------------------
    private void _pendingItemsListView_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      OpenSelectedFiles();
    }

    // ------------------------------------------------------------------------
    // open selected files on key down
    // ------------------------------------------------------------------------
    private void _pendingItemsListView_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyValue == '\r')
      {
        OpenSelectedFiles();
      }
    }

    // ------------------------------------------------------------------------
    // auto column resizing
    // ------------------------------------------------------------------------
    private void _pendingItemsListView_Resize(object sender, EventArgs e)
    {
      /*int width = _pendingItemsListView.Width;
      int count = _pendingItemsListView.Columns.Count;
      if(count>1)
      {
        ColumnHeader header1 = _pendingItemsListView.Columns[0]; 
        ColumnHeader header2 = _pendingItemsListView.Columns[1];
        int newWidth = (width-header1.Width)-4;
        header2.Width = Math.Max(newWidth, 250);
      }
      */
    }

    // ------------------------------------------------------------------------
    // commit dialog for selected files
    // ------------------------------------------------------------------------
    private void OnCommitSelectedFiles(object sender, EventArgs e)
    {
      List<string> array = new List<string>();
      foreach (int index in _pendingItemsListView.SelectedIndices)
      {
        HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
        array.Add(info.fullPath);
      }

      SccProvider.Provider.CommitDialog(array);
    }

    // ------------------------------------------------------------------------
    // diff view for selected file
    // ------------------------------------------------------------------------
    private void OnDiffSelectedFile(object sender, EventArgs e)
    {
      if (_pendingItemsListView.SelectedIndices.Count == 1)
      {
        int index = _pendingItemsListView.SelectedIndices[0];
        HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
        SccProvider.Provider.ShowHgDiffDlg(info.fullPath);
      }
    }

    // ------------------------------------------------------------------------
    // revert dialog for selected files
    // ------------------------------------------------------------------------
    private void OnRevertSelectedFile(object sender, EventArgs e)
    {
        List<string> array = new List<string>();
        foreach (int index in _pendingItemsListView.SelectedIndices)
        {
            HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
            array.Add(info.fullPath);
        }

        SccProvider.Provider.HgRevertFileDlg(array.ToArray());
    }

    // ------------------------------------------------------------------------
    // show history of selected file
    // ------------------------------------------------------------------------
    private void OnShowSelectedFileHistory(object sender, EventArgs e)
    {
      if (_pendingItemsListView.SelectedIndices.Count == 1)
      {
        int index = _pendingItemsListView.SelectedIndices[0];
        HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
        SccProvider.Provider.ShowHgHistoryDlg(info.fullPath);
      }
    }

    // ------------------------------------------------------------------------
    // annotate selected file
    // ------------------------------------------------------------------------
    private void OnAnnotateSelectedFile(object sender, EventArgs e)
    {
      if (_pendingItemsListView.SelectedIndices.Count == 1)
      {
        int index = _pendingItemsListView.SelectedIndices[0];
        HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
        SccProvider.Provider.HgAnnotateDlg(info.fullPath);
      }
    }

    // ------------------------------------------------------------------------
    // open selected files in the editor
    // ------------------------------------------------------------------------
    private void OnOpenSelectedFiles(object sender, EventArgs e)
    {
      OpenSelectedFiles();
    }

    // ------------------------------------------------------------------------
    // update menu flags on list selection changed event
    private void _pendingItemsListView_SelectedIndexChanged(object sender, EventArgs e)
    {
      // enable menu commands
      bool singleSel = false;
      HGLib.HGFileStatus status = HGLib.HGFileStatus.scsUncontrolled;
      if (_pendingItemsListView.SelectedIndices.Count > 0)
      {
        singleSel = (_pendingItemsListView.SelectedIndices.Count == 1) ? true : false;
        int index  = _pendingItemsListView.SelectedIndices[0];
        HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
        status = info.status;
      }  
        
      annotateFileToolStripMenuItem.Visible = singleSel &&  status != HGLib.HGFileStatus.scsAdded &&
                                                            status != HGLib.HGFileStatus.scsRemoved &&
                                                            status != HGLib.HGFileStatus.scsRenamed;
      diffToolStripMenuItem.Visible = singleSel &&          status != HGLib.HGFileStatus.scsRemoved &&
                                                            status != HGLib.HGFileStatus.scsAdded;
      historyToolStripMenuItem.Visible = singleSel &&       status != HGLib.HGFileStatus.scsAdded &&
                                                            status != HGLib.HGFileStatus.scsRenamed;
      openInEditorToolStripMenuItem.Visible =               status != HGLib.HGFileStatus.scsRemoved;
    }
  }
}
