using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;
using HgLib;
using System.Drawing;

namespace VisualHg.Controls
{
    public class VisualHgOptionsControl : UserControl
    {
        private IContainer components;
        private CheckBox addFilesOnLoadCheckBox;
        private CheckBox autoActivateCheckBox;
        private Button selectDiffToolButton;
        private TextBox diffToolPathTextBox;
        private TableLayoutPanel tableLayoutPanel;
        private TextBox tortoiseHgVersionTextBox;
        private TextBox diffToolArgumentsTextBox;
        private OpenFileDialog selectDiffToolDialog;
        private CheckBox autoAddNewFilesCheckBox;
        private TextBox statusImageFileNameTextBox;
        private Button selectStatusImageFileButton;
        private OpenFileDialog selectStatusImageFileDialog;
        private CheckBox autoSaveProjectFilesCheckBox;
        private Button diffToolPresetButton;
        private ContextMenuStrip diffToolPresetMenu;
        private CheckBox projectStatusIncludesChildrenCheckBox;

        public VisualHgOptions Configuration
        {
            get
            {
                return new VisualHgOptions {
                    AutoActivatePlugin = autoActivateCheckBox.Checked,
                    AddFilesOnLoad = addFilesOnLoadCheckBox.Checked,
                    AutoAddNewFiles = autoAddNewFilesCheckBox.Checked,
                    AutoSaveProjectFiles = autoSaveProjectFilesCheckBox.Checked,
                    ProjectStatusIncludesChildren = projectStatusIncludesChildrenCheckBox.Checked,
                    DiffToolPath = diffToolPathTextBox.Text,
                    DiffToolArguments = diffToolArgumentsTextBox.Text,
                    StatusImageFileName = statusImageFileNameTextBox.Text,
                };
            }
            set
            {
                autoActivateCheckBox.Checked = value.AutoActivatePlugin;
                addFilesOnLoadCheckBox.Checked = value.AddFilesOnLoad;
                autoAddNewFilesCheckBox.Checked = value.AutoAddNewFiles;
                autoSaveProjectFilesCheckBox.Checked = value.AutoSaveProjectFiles;
                projectStatusIncludesChildrenCheckBox.Checked = value.ProjectStatusIncludesChildren;
                diffToolPathTextBox.Text = value.DiffToolPath;
                diffToolArgumentsTextBox.Text = value.DiffToolArguments;
                statusImageFileNameTextBox.Text = value.StatusImageFileName;
            }
        }

        public VisualHgOptionsControl()
        {
            InitializeComponent();

            tortoiseHgVersionTextBox.Text = TortoiseHg.Version ?? Resources.TortoiseHgNotFound;

            selectDiffToolButton.Click += SelectDiffTool;
            selectDiffToolDialog.FileOk += (s, e) => diffToolPathTextBox.Text = selectDiffToolDialog.FileName;

            selectStatusImageFileButton.Click += SelectStatusImage;
            selectStatusImageFileDialog.FileOk += (s, e) => statusImageFileNameTextBox.Text = selectStatusImageFileDialog.FileName;

            diffToolPresetButton.Click += (s, e) => ShowDiffToolPresetMenu();
            diffToolPresetMenu.Items.AddRange(CreateDiffToolPresetMenuItems());
        }

        private ToolStripMenuItem[] CreateDiffToolPresetMenuItems()
        {
            return DiffToolPreset.Presets
                .Select(x => new ToolStripMenuItem(x.Name, null, (s, e) => diffToolArgumentsTextBox.Text = x.Arguments))
                .ToArray();
        }

        private void ShowDiffToolPresetMenu()
        {
            var relativeLocation = Point.Add(Point.Empty, new Size(0, diffToolPresetButton.Height));
            var screenLocation = diffToolPresetButton.PointToScreen(relativeLocation);

            diffToolPresetMenu.Show(screenLocation);
        }

        private void SelectDiffTool(object sender, EventArgs e)
        {
            selectDiffToolDialog.FileName = diffToolPathTextBox.Text;
            selectDiffToolDialog.ShowDialog();
        }

        private void SelectStatusImage(object sender, EventArgs e)
        {
            selectStatusImageFileDialog.FileName = diffToolPathTextBox.Text;
            selectStatusImageFileDialog.ShowDialog();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label diffToolPathLabel;
            System.Windows.Forms.Label tortoiseHgVersionLabel;
            System.Windows.Forms.Label noteLabel;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label statusImageFileNameLabel;
            this.addFilesOnLoadCheckBox = new System.Windows.Forms.CheckBox();
            this.autoActivateCheckBox = new System.Windows.Forms.CheckBox();
            this.selectDiffToolButton = new System.Windows.Forms.Button();
            this.diffToolPathTextBox = new System.Windows.Forms.TextBox();
            this.projectStatusIncludesChildrenCheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.diffToolArgumentsTextBox = new System.Windows.Forms.TextBox();
            this.autoAddNewFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.tortoiseHgVersionTextBox = new System.Windows.Forms.TextBox();
            this.statusImageFileNameTextBox = new System.Windows.Forms.TextBox();
            this.selectStatusImageFileButton = new System.Windows.Forms.Button();
            this.autoSaveProjectFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.selectDiffToolDialog = new System.Windows.Forms.OpenFileDialog();
            this.selectStatusImageFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.diffToolPresetButton = new System.Windows.Forms.Button();
            this.diffToolPresetMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            diffToolPathLabel = new System.Windows.Forms.Label();
            tortoiseHgVersionLabel = new System.Windows.Forms.Label();
            noteLabel = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            statusImageFileNameLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // diffToolPathLabel
            // 
            diffToolPathLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            diffToolPathLabel.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(diffToolPathLabel, 3);
            diffToolPathLabel.Location = new System.Drawing.Point(0, 129);
            diffToolPathLabel.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            diffToolPathLabel.Name = "diffToolPathLabel";
            diffToolPathLabel.Size = new System.Drawing.Size(207, 13);
            diffToolPathLabel.TabIndex = 6;
            diffToolPathLabel.Text = "Custom diff tool (leave blank to use KDiff3):";
            // 
            // tortoiseHgVersionLabel
            // 
            tortoiseHgVersionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            tortoiseHgVersionLabel.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(tortoiseHgVersionLabel, 3);
            tortoiseHgVersionLabel.Location = new System.Drawing.Point(0, 277);
            tortoiseHgVersionLabel.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            tortoiseHgVersionLabel.Name = "tortoiseHgVersionLabel";
            tortoiseHgVersionLabel.Size = new System.Drawing.Size(99, 13);
            tortoiseHgVersionLabel.TabIndex = 15;
            tortoiseHgVersionLabel.Text = "TortoiseHg version:";
            // 
            // noteLabel
            // 
            noteLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            noteLabel.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(noteLabel, 3);
            noteLabel.Location = new System.Drawing.Point(16, 105);
            noteLabel.Margin = new System.Windows.Forms.Padding(16, 0, 0, 0);
            noteLabel.Name = "noteLabel";
            noteLabel.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
            noteLabel.Size = new System.Drawing.Size(212, 14);
            noteLabel.TabIndex = 5;
            noteLabel.Text = "NOTE: This may be slow with large projects";
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(label1, 3);
            label1.Location = new System.Drawing.Point(0, 179);
            label1.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(370, 13);
            label1.TabIndex = 9;
            label1.Text = "Diff arguments (use %PathA%, %NameA%, %PathB%, %NameB%, no quotes):";
            // 
            // statusImageFileNameLabel
            // 
            statusImageFileNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            statusImageFileNameLabel.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(statusImageFileNameLabel, 3);
            statusImageFileNameLabel.Location = new System.Drawing.Point(0, 227);
            statusImageFileNameLabel.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            statusImageFileNameLabel.Name = "statusImageFileNameLabel";
            statusImageFileNameLabel.Size = new System.Drawing.Size(165, 13);
            statusImageFileNameLabel.TabIndex = 12;
            statusImageFileNameLabel.Text = "Status image file (requires restart):";
            // 
            // addFilesOnLoadCheckBox
            // 
            this.addFilesOnLoadCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.addFilesOnLoadCheckBox.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.addFilesOnLoadCheckBox, 3);
            this.addFilesOnLoadCheckBox.Location = new System.Drawing.Point(0, 21);
            this.addFilesOnLoadCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.addFilesOnLoadCheckBox.Name = "addFilesOnLoadCheckBox";
            this.addFilesOnLoadCheckBox.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.addFilesOnLoadCheckBox.Size = new System.Drawing.Size(274, 21);
            this.addFilesOnLoadCheckBox.TabIndex = 1;
            this.addFilesOnLoadCheckBox.Text = "Add not tracked files to repository on project opening";
            this.addFilesOnLoadCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoActivateCheckBox
            // 
            this.autoActivateCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoActivateCheckBox.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.autoActivateCheckBox, 3);
            this.autoActivateCheckBox.Location = new System.Drawing.Point(0, 0);
            this.autoActivateCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.autoActivateCheckBox.Name = "autoActivateCheckBox";
            this.autoActivateCheckBox.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.autoActivateCheckBox.Size = new System.Drawing.Size(255, 21);
            this.autoActivateCheckBox.TabIndex = 0;
            this.autoActivateCheckBox.Text = "Automatically activate plugin on solution opening";
            this.autoActivateCheckBox.UseVisualStyleBackColor = true;
            // 
            // selectDiffToolButton
            // 
            this.selectDiffToolButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.selectDiffToolButton.AutoSize = true;
            this.selectDiffToolButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.selectDiffToolButton.Location = new System.Drawing.Point(430, 142);
            this.selectDiffToolButton.Margin = new System.Windows.Forms.Padding(0);
            this.selectDiffToolButton.Name = "selectDiffToolButton";
            this.selectDiffToolButton.Padding = new System.Windows.Forms.Padding(2);
            this.selectDiffToolButton.Size = new System.Drawing.Size(30, 27);
            this.selectDiffToolButton.TabIndex = 8;
            this.selectDiffToolButton.Text = "...";
            this.selectDiffToolButton.UseVisualStyleBackColor = true;
            // 
            // diffToolPathTextBox
            // 
            this.diffToolPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.diffToolPathTextBox.Location = new System.Drawing.Point(0, 145);
            this.diffToolPathTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.diffToolPathTextBox.Name = "diffToolPathTextBox";
            this.diffToolPathTextBox.Size = new System.Drawing.Size(423, 20);
            this.diffToolPathTextBox.TabIndex = 7;
            // 
            // projectStatusIncludesChildrenCheckBox
            // 
            this.projectStatusIncludesChildrenCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.projectStatusIncludesChildrenCheckBox.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.projectStatusIncludesChildrenCheckBox, 3);
            this.projectStatusIncludesChildrenCheckBox.Location = new System.Drawing.Point(0, 84);
            this.projectStatusIncludesChildrenCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.projectStatusIncludesChildrenCheckBox.Name = "projectStatusIncludesChildrenCheckBox";
            this.projectStatusIncludesChildrenCheckBox.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.projectStatusIncludesChildrenCheckBox.Size = new System.Drawing.Size(251, 21);
            this.projectStatusIncludesChildrenCheckBox.TabIndex = 4;
            this.projectStatusIncludesChildrenCheckBox.Text = "Include child items for determining project status";
            this.projectStatusIncludesChildrenCheckBox.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 3;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 7F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Controls.Add(this.autoActivateCheckBox, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.selectDiffToolButton, 2, 8);
            this.tableLayoutPanel.Controls.Add(this.diffToolPathTextBox, 0, 8);
            this.tableLayoutPanel.Controls.Add(diffToolPathLabel, 0, 7);
            this.tableLayoutPanel.Controls.Add(this.projectStatusIncludesChildrenCheckBox, 0, 4);
            this.tableLayoutPanel.Controls.Add(this.addFilesOnLoadCheckBox, 0, 1);
            this.tableLayoutPanel.Controls.Add(noteLabel, 0, 5);
            this.tableLayoutPanel.Controls.Add(label1, 0, 10);
            this.tableLayoutPanel.Controls.Add(this.diffToolArgumentsTextBox, 0, 11);
            this.tableLayoutPanel.Controls.Add(this.autoAddNewFilesCheckBox, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.tortoiseHgVersionTextBox, 0, 17);
            this.tableLayoutPanel.Controls.Add(tortoiseHgVersionLabel, 0, 16);
            this.tableLayoutPanel.Controls.Add(statusImageFileNameLabel, 0, 13);
            this.tableLayoutPanel.Controls.Add(this.statusImageFileNameTextBox, 0, 14);
            this.tableLayoutPanel.Controls.Add(this.selectStatusImageFileButton, 2, 14);
            this.tableLayoutPanel.Controls.Add(this.autoSaveProjectFilesCheckBox, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.diffToolPresetButton, 2, 11);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 19;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(460, 334);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // diffToolArgumentsTextBox
            // 
            this.diffToolArgumentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.diffToolArgumentsTextBox.Location = new System.Drawing.Point(0, 194);
            this.diffToolArgumentsTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.diffToolArgumentsTextBox.Name = "diffToolArgumentsTextBox";
            this.diffToolArgumentsTextBox.Size = new System.Drawing.Size(423, 20);
            this.diffToolArgumentsTextBox.TabIndex = 10;
            // 
            // autoAddNewFilesCheckBox
            // 
            this.autoAddNewFilesCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoAddNewFilesCheckBox.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.autoAddNewFilesCheckBox, 3);
            this.autoAddNewFilesCheckBox.Location = new System.Drawing.Point(0, 42);
            this.autoAddNewFilesCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.autoAddNewFilesCheckBox.Name = "autoAddNewFilesCheckBox";
            this.autoAddNewFilesCheckBox.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.autoAddNewFilesCheckBox.Size = new System.Drawing.Size(213, 21);
            this.autoAddNewFilesCheckBox.TabIndex = 2;
            this.autoAddNewFilesCheckBox.Text = "Automatically add new files to repository";
            this.autoAddNewFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // tortoiseHgVersionTextBox
            // 
            this.tortoiseHgVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.SetColumnSpan(this.tortoiseHgVersionTextBox, 3);
            this.tortoiseHgVersionTextBox.Location = new System.Drawing.Point(0, 290);
            this.tortoiseHgVersionTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.tortoiseHgVersionTextBox.Name = "tortoiseHgVersionTextBox";
            this.tortoiseHgVersionTextBox.ReadOnly = true;
            this.tortoiseHgVersionTextBox.Size = new System.Drawing.Size(460, 20);
            this.tortoiseHgVersionTextBox.TabIndex = 16;
            this.tortoiseHgVersionTextBox.TabStop = false;
            // 
            // statusImageFileNameTextBox
            // 
            this.statusImageFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.statusImageFileNameTextBox.Location = new System.Drawing.Point(0, 243);
            this.statusImageFileNameTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.statusImageFileNameTextBox.Name = "statusImageFileNameTextBox";
            this.statusImageFileNameTextBox.Size = new System.Drawing.Size(423, 20);
            this.statusImageFileNameTextBox.TabIndex = 13;
            // 
            // selectStatusImageFileButton
            // 
            this.selectStatusImageFileButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.selectStatusImageFileButton.AutoSize = true;
            this.selectStatusImageFileButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.selectStatusImageFileButton.Location = new System.Drawing.Point(430, 240);
            this.selectStatusImageFileButton.Margin = new System.Windows.Forms.Padding(0);
            this.selectStatusImageFileButton.Name = "selectStatusImageFileButton";
            this.selectStatusImageFileButton.Padding = new System.Windows.Forms.Padding(2);
            this.selectStatusImageFileButton.Size = new System.Drawing.Size(30, 27);
            this.selectStatusImageFileButton.TabIndex = 14;
            this.selectStatusImageFileButton.Text = "...";
            this.selectStatusImageFileButton.UseVisualStyleBackColor = true;
            // 
            // autoSaveProjectFilesCheckBox
            // 
            this.autoSaveProjectFilesCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoSaveProjectFilesCheckBox.AutoSize = true;
            this.autoSaveProjectFilesCheckBox.Location = new System.Drawing.Point(0, 63);
            this.autoSaveProjectFilesCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.autoSaveProjectFilesCheckBox.Name = "autoSaveProjectFilesCheckBox";
            this.autoSaveProjectFilesCheckBox.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.autoSaveProjectFilesCheckBox.Size = new System.Drawing.Size(272, 21);
            this.autoSaveProjectFilesCheckBox.TabIndex = 3;
            this.autoSaveProjectFilesCheckBox.Text = "Save project files before opening TortoiseHg dialogs";
            this.autoSaveProjectFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // selectDiffToolDialog
            // 
            this.selectDiffToolDialog.AddExtension = false;
            this.selectDiffToolDialog.DefaultExt = "exe";
            this.selectDiffToolDialog.Filter = "Executable files|*.exe|All files|*.*";
            this.selectDiffToolDialog.ShowReadOnly = true;
            this.selectDiffToolDialog.Title = "Select diff tool";
            // 
            // selectStatusImageFileDialog
            // 
            this.selectStatusImageFileDialog.AddExtension = false;
            this.selectStatusImageFileDialog.Filter = "Image files|*.bmp;*.png;*.gif|All files|*.*";
            this.selectStatusImageFileDialog.ShowReadOnly = true;
            this.selectStatusImageFileDialog.Title = "Select status image file";
            // 
            // diffToolPresetButton
            // 
            this.diffToolPresetButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.diffToolPresetButton.AutoSize = true;
            this.diffToolPresetButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.diffToolPresetButton.Font = new System.Drawing.Font("Marlett", 8.25F);
            this.diffToolPresetButton.Location = new System.Drawing.Point(430, 192);
            this.diffToolPresetButton.Margin = new System.Windows.Forms.Padding(0);
            this.diffToolPresetButton.Name = "diffToolPresetButton";
            this.diffToolPresetButton.Padding = new System.Windows.Forms.Padding(2);
            this.diffToolPresetButton.Size = new System.Drawing.Size(30, 25);
            this.diffToolPresetButton.TabIndex = 11;
            this.diffToolPresetButton.Text = "u";
            this.diffToolPresetButton.UseVisualStyleBackColor = true;
            // 
            // diffToolPresetMenu
            // 
            this.diffToolPresetMenu.Name = "diffToolPresetMenu";
            this.diffToolPresetMenu.Size = new System.Drawing.Size(153, 26);
            // 
            // VisualHgOptionsControl
            // 
            this.AllowDrop = true;
            this.AutoScroll = true;
            this.Controls.Add(this.tableLayoutPanel);
            this.MaximumSize = new System.Drawing.Size(460, 334);
            this.MinimumSize = new System.Drawing.Size(460, 334);
            this.Name = "VisualHgOptionsControl";
            this.Size = new System.Drawing.Size(460, 334);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
