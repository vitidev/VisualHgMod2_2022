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
        private PendingItemsListView _pendingItemsListView;

        public HGPendingChangesToolWindowControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
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
          this._pendingItemsListView = new VisualHG.PendingItemsListView();
          this.columnHeaderStatus = new System.Windows.Forms.ColumnHeader();
          this.columnHeaderFileName = new System.Windows.Forms.ColumnHeader();
          this.SuspendLayout();
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
          // HGPendingChangesToolWindowControl
          // 
          this.BackColor = System.Drawing.SystemColors.Window;
          this.Controls.Add(this._pendingItemsListView);
          this.Name = "HGPendingChangesToolWindowControl";
          resources.ApplyResources(this, "$this");
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
          foreach(int index in  _pendingItemsListView.SelectedIndices)
          {
            HGLib.HGFileStatusInfo info = _pendingItemsListView._list[index];
            try{
              VsShellUtilities.OpenDocument(SccProvider.ServiceProvider(), info.fullPath);
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
    }
}
