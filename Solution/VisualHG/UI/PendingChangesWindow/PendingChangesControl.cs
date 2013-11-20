using System.Linq;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using HgLib;
using Microsoft.VisualStudio.Shell;

namespace VisualHg
{
    public class PendingChangesControl : UserControl
    {
        private IContainer components;
        private HgFileInfoListView fileListView;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem commitMenuItem;
        private ToolStripMenuItem diffMenuItem;
        private ToolStripMenuItem revertMenuItem;
        private ToolStripMenuItem historyMenuItem;
        private ColumnHeader statusColumnHeader;
        private ColumnHeader pathColumnHeader;
        private ToolStripMenuItem openMenuItem;

        private SccProvider sccProvider;


        public PendingChangesControl()
        {
            InitializeComponent();
            SetMenuItemImages();

            commitMenuItem.Click += ShowCommitWindow;
            diffMenuItem.Click += ShowDiffWindow;
            revertMenuItem.Click += ShowRevertWindow;
            historyMenuItem.Click += ShowHistoryWindow;
            openMenuItem.Click += OpenSelectedFiles;

            fileListView.DoubleClick += OpenSelectedFiles;
            fileListView.KeyDown += OnFilesListViewKeyDown;
            fileListView.ItemSelectionChanged += UpdateMenuItemVisibility;

            sccProvider = Package.GetGlobalService(typeof(IServiceProvider)) as SccProvider;
        }

        private void SetMenuItemImages()
        {
            using (var menuImageList = ImageMapper.CreateMenuImageList())
            {
                commitMenuItem.Image = menuImageList.Images[0];
                historyMenuItem.Image = menuImageList.Images[1];
                diffMenuItem.Image = menuImageList.Images[4];
                revertMenuItem.Image = menuImageList.Images[7];
                openMenuItem.Image = menuImageList.Images[8];
            }
        }


        public void SetFiles(HgFileInfo[] files)
        {
            fileListView.Files = files;
        }


        private void OpenSelectedFiles()
        {
            foreach (var fileName in fileListView.SelectedFiles)
            {
                try
                {
                    VsShellUtilities.OpenDocument(sccProvider, fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "VisualHg", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void OpenSelectedFiles(object sender, EventArgs e)
        {
            OpenSelectedFiles();
        }

        private void ShowCommitWindow(object sender, EventArgs e)
        {
            sccProvider.ShowCommitWindow(fileListView.SelectedFiles);
        }

        private void ShowDiffWindow(object sender, EventArgs e)
        {
            if (fileListView.SelectedIndices.Count == 1)
            {
                sccProvider.ShowDiffWindow(fileListView.SelectedFiles[0]);
            }
        }

        private void ShowRevertWindow(object sender, EventArgs e)
        {
            sccProvider.ShowRevertWindow(fileListView.SelectedFiles);
        }

        private void ShowHistoryWindow(object sender, EventArgs e)
        {
            if (fileListView.SelectedIndices.Count == 1)
            {
                sccProvider.ShowHistoryWindow(fileListView.SelectedFiles[0]);
            }
        }


        private void OnFilesListViewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OpenSelectedFiles();
            }
        }

        private void UpdateMenuItemVisibility(object sender, EventArgs e)
        {
            var single = fileListView.SelectedIndices.Count == 1;

            var status = HgFileStatus.NotTracked;
            
            if (fileListView.SelectedIndices.Count > 0)
            {
                status = fileListView.SelectedIndices.Cast<int>()
                    .Select(x => fileListView.Files[x].Status)
                    .Aggregate((x, y) => x | y);
            }

            commitMenuItem.Visible = StatusMatches(status, HgFileStatus.Pending);
            revertMenuItem.Visible = StatusMatches(status, HgFileStatus.Pending);
            
            diffMenuItem.Visible = single && StatusMatches(status, HgFileStatus.Comparable);
            historyMenuItem.Visible = single && StatusMatches(status, HgFileStatus.Tracked);
            openMenuItem.Visible = !StatusMatches(status, HgFileStatus.Deleted);
        }

        private bool StatusMatches(HgFileStatus status, HgFileStatus pattern)
        {
            return (status & pattern) > 0;
        }


        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PendingChangesControl));
            System.Windows.Forms.ColumnHeader fileNameColumnHeader;
            System.Windows.Forms.ColumnHeader rootColumnHeader;
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.commitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.historyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileListView = new VisualHg.HgFileInfoListView();
            this.statusColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pathColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            fileNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            rootColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitMenuItem,
            this.diffMenuItem,
            this.revertMenuItem,
            this.historyMenuItem,
            this.openMenuItem});
            this.contextMenu.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenu, "contextMenu");
            // 
            // commitMenuItem
            // 
            this.commitMenuItem.Name = "commitMenuItem";
            resources.ApplyResources(this.commitMenuItem, "commitMenuItem");
            // 
            // diffMenuItem
            // 
            this.diffMenuItem.Name = "diffMenuItem";
            resources.ApplyResources(this.diffMenuItem, "diffMenuItem");
            // 
            // revertMenuItem
            // 
            this.revertMenuItem.Name = "revertMenuItem";
            resources.ApplyResources(this.revertMenuItem, "revertMenuItem");
            // 
            // historyMenuItem
            // 
            this.historyMenuItem.Name = "historyMenuItem";
            resources.ApplyResources(this.historyMenuItem, "historyMenuItem");
            // 
            // openMenuItem
            // 
            this.openMenuItem.Name = "openMenuItem";
            resources.ApplyResources(this.openMenuItem, "openMenuItem");
            // 
            // fileListView
            // 
            this.fileListView.AllowColumnReorder = true;
            this.fileListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fileListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            fileNameColumnHeader,
            this.statusColumnHeader,
            rootColumnHeader,
            this.pathColumnHeader});
            this.fileListView.ContextMenuStrip = this.contextMenu;
            resources.ApplyResources(this.fileListView, "fileListView");
            this.fileListView.FullRowSelect = true;
            this.fileListView.HideSelection = false;
            this.fileListView.Name = "fileListView";
            this.fileListView.ShowGroups = false;
            this.fileListView.UseCompatibleStateImageBehavior = false;
            this.fileListView.View = System.Windows.Forms.View.Details;
            this.fileListView.VirtualMode = true;
            // 
            // fileNameColumnHeader
            // 
            resources.ApplyResources(fileNameColumnHeader, "fileNameColumnHeader");
            // 
            // statusColumnHeader
            // 
            resources.ApplyResources(this.statusColumnHeader, "statusColumnHeader");
            // 
            // rootColumnHeader
            // 
            resources.ApplyResources(rootColumnHeader, "rootColumnHeader");
            // 
            // pathColumnHeader
            // 
            resources.ApplyResources(this.pathColumnHeader, "pathColumnHeader");
            // 
            // PendingChangesControl
            // 
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.fileListView);
            this.Name = "PendingChangesControl";
            resources.ApplyResources(this, "$this");
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
