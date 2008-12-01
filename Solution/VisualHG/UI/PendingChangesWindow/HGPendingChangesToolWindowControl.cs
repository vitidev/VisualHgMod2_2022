using System;
using System.Globalization;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using IServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace VisualHG
{
    /// <summary>
    /// Summary description for SccProviderToolWindowControl.
    /// </summary>
    public class HGPendingChangesToolWindowControl : System.Windows.Forms.UserControl
    {
        private ColumnHeader columnHeaderStatus;
        private ColumnHeader columnHeaderFileName;
        private ColumnHeader columnHeaderDirectory;
        private ListView _pendingItemsListView;
        //HGStatusTracker _sccStatusTracker;

        public HGPendingChangesToolWindowControl()
        {
            SccProvider sccProvider = (SccProvider)GetService(typeof(SccProvider));
            SccProviderService sccProviderService = (SccProviderService)GetService(typeof(SccProviderService));

            //_sccStatusTracker = sccProvider.._sccStatusTracker;
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // Subscribe to storrage events
            //_sccStatusTracker.HGStatusChanged += new HGLib.HGStatusChangedEvent(UpdatePendingList);
        }

        public void RefreshNodesGlyphs()
        {
//            var solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
//            var projectList = _sccProvider.GetLoadedControllableProjects();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HGPendingChangesToolWindowControl));
            this._pendingItemsListView = new System.Windows.Forms.ListView();
            this.columnHeaderStatus = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderFileName = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderDirectory = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // _pendingItemsListView
            // 
            this._pendingItemsListView.CheckBoxes = true;
            this._pendingItemsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStatus,
            this.columnHeaderFileName,
            this.columnHeaderDirectory});
            resources.ApplyResources(this._pendingItemsListView, "_pendingItemsListView");
            this._pendingItemsListView.GridLines = true;
            this._pendingItemsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._pendingItemsListView.HideSelection = false;
            this._pendingItemsListView.Name = "_pendingItemsListView";
            this._pendingItemsListView.UseCompatibleStateImageBehavior = false;
            this._pendingItemsListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderStatus
            // 
            resources.ApplyResources(this.columnHeaderStatus, "columnHeaderStatus");
            // 
            // columnHeaderFileName
            // 
            resources.ApplyResources(this.columnHeaderFileName, "columnHeaderFileName");
            // 
            // columnHeaderDirectory
            // 
            resources.ApplyResources(this.columnHeaderDirectory, "columnHeaderDirectory");
            // 
            // SccProviderToolWindowControl
            // 
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this._pendingItemsListView);
            this.Name = "SccProviderToolWindowControl";
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);

        }
        #endregion
    }
}
