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
      this.commitToolStripMenuItem.Image = menuImageList.Images[0];
      this.diffToolStripMenuItem.Image = menuImageList.Images[4];
      this.revertToolStripMenuItem.Image = menuImageList.Images[8];
      this.historyToolStripMenuItem.Image = menuImageList.Images[9];
      this.annotateFileToolStripMenuItem.Image = menuImageList.Images[7];
      this.openInEditorToolStripMenuItem.Image = menuImageList.Images[1];
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
      this._pendingItemsListView = new VisualHG.PendingItemsListView();
      this.columnHeaderStatus = new System.Windows.Forms.ColumnHeader();
      this.columnHeaderFileName = new System.Windows.Forms.ColumnHeader();
      this.diffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.revertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.historyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.annotateFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.openInEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
      this.commitToolStripMenuItem.Click += new System.EventHandler(this.OnCommitSelected);
      // 
      // _pendingItemsListView
      // 
      this._pendingItemsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStatus,
            this.columnHeaderFileName});
      resources.ApplyResources(this._pendingItemsListView, "_pendingItemsListView");
      this._pendingItemsListView.FullRowSelect = true;
      this._pendingItemsListView.GridLines = true;
      this._pendingItemsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this._pendingItemsListView.HideSelection = false;
      this._pendingItemsListView.Name = "_pendingItemsListView";
      this._pendingItemsListView.ShowGroups = false;
      this._pendingItemsListView.UseCompatibleStateImageBehavior = false;
      this._pendingItemsListView.View = System.Windows.Forms.View.Details;
      this._pendingItemsListView.VirtualMode = true;
      this._pendingItemsListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this._pendingItemsListView_MouseDoubleClick);
      this._pendingItemsListView.Resize += new System.EventHandler(this._pendingItemsListView_Resize);
      this._pendingItemsListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this._pendingItemsListView_MouseClick);
      this._pendingItemsListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this._pendingItemsListView_KeyDown);
      // 
      // columnHeaderStatus
      // 
      resources.ApplyResources(this.columnHeaderStatus, "columnHeaderStatus");
      // 
      // columnHeaderFileName
      // 
      resources.ApplyResources(this.columnHeaderFileName, "columnHeaderFileName");
      // 
      // diffToolStripMenuItem
      // 
      this.diffToolStripMenuItem.Name = "diffToolStripMenuItem";
      resources.ApplyResources(this.diffToolStripMenuItem, "diffToolStripMenuItem");
      this.diffToolStripMenuItem.Click += new System.EventHandler(this.OnDiffSelected);
      // 
      // revertToolStripMenuItem
      // 
      this.revertToolStripMenuItem.Name = "revertToolStripMenuItem";
      resources.ApplyResources(this.revertToolStripMenuItem, "revertToolStripMenuItem");
      this.revertToolStripMenuItem.Click += new System.EventHandler(this.OnRevertFile);
      // 
      // historyToolStripMenuItem
      // 
      this.historyToolStripMenuItem.Name = "historyToolStripMenuItem";
      resources.ApplyResources(this.historyToolStripMenuItem, "historyToolStripMenuItem");
      this.historyToolStripMenuItem.Click += new System.EventHandler(this.OnShowHistoryOfSelected);
      // 
      // annotateFileToolStripMenuItem
      // 
      this.annotateFileToolStripMenuItem.Name = "annotateFileToolStripMenuItem";
      resources.ApplyResources(this.annotateFileToolStripMenuItem, "annotateFileToolStripMenuItem");
      this.annotateFileToolStripMenuItem.Click += new System.EventHandler(this.OnAnnotateFile);
      // 
      // oPenInEditorToolStripMenuItem
      // 
      this.openInEditorToolStripMenuItem.Name = "oPenInEditorToolStripMenuItem";
      resources.ApplyResources(this.openInEditorToolStripMenuItem, "oPenInEditorToolStripMenuItem");
      this.openInEditorToolStripMenuItem.Click += new System.EventHandler(this.OnOpenFileInInEditor);
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

    public void UpdatePendingList(HGStatusTracker tracker)
    {
      _pendingItemsListView.UpdatePendingList(tracker);
    }

    // open selected file(s)
    void OpenSelectedFiles()
    {
      foreach (int index in _pendingItemsListView.SelectedIndices)
      {
        HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
        try
        {
          VsShellUtilities.OpenDocument(SccProvider.ServiceProvider, info.fullPath);
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Open File failed");
        }
      }
    }

    private void _pendingItemsListView_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      OpenSelectedFiles();
    }

    private void _pendingItemsListView_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyValue == '\r')
      {
        OpenSelectedFiles();
      }
    }

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

    private void _pendingItemsListView_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        if (_pendingItemsListView.SelectedIndices.Count > 0)
        {
          bool singleSel = (_pendingItemsListView.SelectedIndices.Count == 1) ? true : false;
          revertToolStripMenuItem.Visible = singleSel;
          annotateFileToolStripMenuItem.Visible = singleSel;
          diffToolStripMenuItem.Visible = singleSel;
          historyToolStripMenuItem.Visible = singleSel;
          pendingChangesContextMenu.Show(_pendingItemsListView, e.Location);
        }
      }
    }

    private void OnCommitSelected(object sender, EventArgs e)
    {
      List<string> array = new List<string>();
      foreach (int index in _pendingItemsListView.SelectedIndices)
      {
        HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
        array.Add(info.fullPath);
      }

      SccProvider.ServiceProvider.HgCommitSelected(array);
    }

    private void OnDiffSelected(object sender, EventArgs e)
    {
      if (_pendingItemsListView.SelectedIndices.Count == 1)
      {
        int index = _pendingItemsListView.SelectedIndices[0];
        HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
        SccProvider.ServiceProvider.ShowHgDiffDlg(info.fullPath);
      }
    }

    private void OnRevertFile(object sender, EventArgs e)
    {
      if (_pendingItemsListView.SelectedIndices.Count == 1)
      {
        int index = _pendingItemsListView.SelectedIndices[0];
        HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
        SccProvider.ServiceProvider.HgRevertFileDlg(info.fullPath);
      }
    }

    private void OnShowHistoryOfSelected(object sender, EventArgs e)
    {
      if (_pendingItemsListView.SelectedIndices.Count == 1)
      {
        int index = _pendingItemsListView.SelectedIndices[0];
        HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
        SccProvider.ServiceProvider.ShowHgHistoryDlg(info.fullPath);
      }
    }

    private void OnAnnotateFile(object sender, EventArgs e)
    {
      if (_pendingItemsListView.SelectedIndices.Count == 1)
      {
        int index = _pendingItemsListView.SelectedIndices[0];
        HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
        SccProvider.ServiceProvider.HgAnnotateDlg(info.fullPath);
      }
    }

    private void OnOpenFileInInEditor(object sender, EventArgs e)
    {
      OpenSelectedFiles();
    }
  }
}
