using System;
using System.ComponentModel;
using System.Windows.Forms;
using HgLib;

namespace VisualHg.Controls
{
    public class VisualHgOptionsControl : UserControl
    {
        private Container components;
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
        private CheckBox projectStatusIncludesChildrenCheckBox;

        public VisualHgOptions Configuration
        {
            get
            {
                return new VisualHgOptions {
                    AutoActivatePlugin = autoActivateCheckBox.Checked,
                    AddFilesOnLoad = addFilesOnLoadCheckBox.Checked,
                    AutoAddNewFiles = autoAddNewFilesCheckBox.Checked,
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
                projectStatusIncludesChildrenCheckBox.Checked = value.ProjectStatusIncludesChildren;
                diffToolPathTextBox.Text = value.DiffToolPath;
                diffToolArgumentsTextBox.Text = value.DiffToolArguments;
                statusImageFileNameTextBox.Text = value.StatusImageFileName;
            }
        }

        public VisualHgOptionsControl()
        {
            InitializeComponent();

            tortoiseHgVersionTextBox.Text = TortoiseHg.Version ?? "TortoiseHg was not found";

            selectDiffToolButton.Click += SelectDiffTool;
            selectDiffToolDialog.FileOk += (s, e) => diffToolPathTextBox.Text = selectDiffToolDialog.FileName;

            selectStatusImageFileButton.Click += SelectStatusImage;
            selectStatusImageFileDialog.FileOk += (s, e) => statusImageFileNameTextBox.Text = selectStatusImageFileDialog.FileName;
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
            this.selectDiffToolDialog = new System.Windows.Forms.OpenFileDialog();
            this.selectStatusImageFileDialog = new System.Windows.Forms.OpenFileDialog();
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
            diffToolPathLabel.Location = new System.Drawing.Point(0, 109);
            diffToolPathLabel.Margin = new System.Windows.Forms.Padding(0);
            diffToolPathLabel.Name = "diffToolPathLabel";
            diffToolPathLabel.Size = new System.Drawing.Size(207, 13);
            diffToolPathLabel.TabIndex = 5;
            diffToolPathLabel.Text = "Custom diff tool (leave blank to use kdiff3):";
            // 
            // tortoiseHgVersionLabel
            // 
            tortoiseHgVersionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            tortoiseHgVersionLabel.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(tortoiseHgVersionLabel, 3);
            tortoiseHgVersionLabel.Location = new System.Drawing.Point(0, 259);
            tortoiseHgVersionLabel.Margin = new System.Windows.Forms.Padding(0);
            tortoiseHgVersionLabel.Name = "tortoiseHgVersionLabel";
            tortoiseHgVersionLabel.Size = new System.Drawing.Size(99, 13);
            tortoiseHgVersionLabel.TabIndex = 13;
            tortoiseHgVersionLabel.Text = "TortoiseHg version:";
            // 
            // noteLabel
            // 
            noteLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            noteLabel.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(noteLabel, 3);
            noteLabel.Location = new System.Drawing.Point(16, 85);
            noteLabel.Margin = new System.Windows.Forms.Padding(16, 0, 0, 0);
            noteLabel.Name = "noteLabel";
            noteLabel.Size = new System.Drawing.Size(212, 13);
            noteLabel.TabIndex = 4;
            noteLabel.Text = "NOTE: This may be slow with large projects";
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(label1, 3);
            label1.Location = new System.Drawing.Point(0, 159);
            label1.Margin = new System.Windows.Forms.Padding(0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(370, 13);
            label1.TabIndex = 8;
            label1.Text = "Diff arguments (use %PathA%, %NameA%, %PathB%, %NameB%, no quotes):";
            // 
            // statusImageFileNameLabel
            // 
            statusImageFileNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            statusImageFileNameLabel.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(statusImageFileNameLabel, 3);
            statusImageFileNameLabel.Location = new System.Drawing.Point(0, 209);
            statusImageFileNameLabel.Margin = new System.Windows.Forms.Padding(0);
            statusImageFileNameLabel.Name = "statusImageFileNameLabel";
            statusImageFileNameLabel.Size = new System.Drawing.Size(165, 13);
            statusImageFileNameLabel.TabIndex = 10;
            statusImageFileNameLabel.Text = "Status image file (requires restart):";
            // 
            // addFilesOnLoadCheckBox
            // 
            this.addFilesOnLoadCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.addFilesOnLoadCheckBox.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.addFilesOnLoadCheckBox, 3);
            this.addFilesOnLoadCheckBox.Location = new System.Drawing.Point(0, 23);
            this.addFilesOnLoadCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.addFilesOnLoadCheckBox.Name = "addFilesOnLoadCheckBox";
            this.addFilesOnLoadCheckBox.Size = new System.Drawing.Size(274, 17);
            this.addFilesOnLoadCheckBox.TabIndex = 1;
            this.addFilesOnLoadCheckBox.Text = "Add not tracked files to repository on project opening";
            this.addFilesOnLoadCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoActivateCheckBox
            // 
            this.autoActivateCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoActivateCheckBox.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.autoActivateCheckBox, 3);
            this.autoActivateCheckBox.Location = new System.Drawing.Point(0, 2);
            this.autoActivateCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.autoActivateCheckBox.Name = "autoActivateCheckBox";
            this.autoActivateCheckBox.Size = new System.Drawing.Size(255, 17);
            this.autoActivateCheckBox.TabIndex = 0;
            this.autoActivateCheckBox.Text = "Automatically activate plugin on solution opening";
            this.autoActivateCheckBox.UseVisualStyleBackColor = true;
            // 
            // selectDiffToolButton
            // 
            this.selectDiffToolButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.selectDiffToolButton.Location = new System.Drawing.Point(430, 124);
            this.selectDiffToolButton.Margin = new System.Windows.Forms.Padding(0);
            this.selectDiffToolButton.Name = "selectDiffToolButton";
            this.selectDiffToolButton.Size = new System.Drawing.Size(30, 26);
            this.selectDiffToolButton.TabIndex = 7;
            this.selectDiffToolButton.Text = "...";
            this.selectDiffToolButton.UseVisualStyleBackColor = true;
            // 
            // diffToolPathTextBox
            // 
            this.diffToolPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.diffToolPathTextBox.Location = new System.Drawing.Point(0, 127);
            this.diffToolPathTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.diffToolPathTextBox.Name = "diffToolPathTextBox";
            this.diffToolPathTextBox.Size = new System.Drawing.Size(423, 20);
            this.diffToolPathTextBox.TabIndex = 6;
            // 
            // projectStatusIncludesChildrenCheckBox
            // 
            this.projectStatusIncludesChildrenCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.projectStatusIncludesChildrenCheckBox.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.projectStatusIncludesChildrenCheckBox, 3);
            this.projectStatusIncludesChildrenCheckBox.Location = new System.Drawing.Point(0, 65);
            this.projectStatusIncludesChildrenCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.projectStatusIncludesChildrenCheckBox.Name = "projectStatusIncludesChildrenCheckBox";
            this.projectStatusIncludesChildrenCheckBox.Size = new System.Drawing.Size(251, 17);
            this.projectStatusIncludesChildrenCheckBox.TabIndex = 3;
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
            this.tableLayoutPanel.Controls.Add(this.selectDiffToolButton, 2, 7);
            this.tableLayoutPanel.Controls.Add(this.diffToolPathTextBox, 0, 7);
            this.tableLayoutPanel.Controls.Add(diffToolPathLabel, 0, 6);
            this.tableLayoutPanel.Controls.Add(this.projectStatusIncludesChildrenCheckBox, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.addFilesOnLoadCheckBox, 0, 1);
            this.tableLayoutPanel.Controls.Add(noteLabel, 0, 4);
            this.tableLayoutPanel.Controls.Add(label1, 0, 9);
            this.tableLayoutPanel.Controls.Add(this.diffToolArgumentsTextBox, 0, 10);
            this.tableLayoutPanel.Controls.Add(this.autoAddNewFilesCheckBox, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.tortoiseHgVersionTextBox, 0, 16);
            this.tableLayoutPanel.Controls.Add(tortoiseHgVersionLabel, 0, 15);
            this.tableLayoutPanel.Controls.Add(statusImageFileNameLabel, 0, 12);
            this.tableLayoutPanel.Controls.Add(this.statusImageFileNameTextBox, 0, 13);
            this.tableLayoutPanel.Controls.Add(this.selectStatusImageFileButton, 2, 13);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 18;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(460, 334);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // diffToolArgumentsTextBox
            // 
            this.diffToolArgumentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.SetColumnSpan(this.diffToolArgumentsTextBox, 3);
            this.diffToolArgumentsTextBox.Location = new System.Drawing.Point(0, 177);
            this.diffToolArgumentsTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.diffToolArgumentsTextBox.Name = "diffToolArgumentsTextBox";
            this.diffToolArgumentsTextBox.Size = new System.Drawing.Size(460, 20);
            this.diffToolArgumentsTextBox.TabIndex = 9;
            // 
            // autoAddNewFilesCheckBox
            // 
            this.autoAddNewFilesCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoAddNewFilesCheckBox.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.autoAddNewFilesCheckBox, 3);
            this.autoAddNewFilesCheckBox.Location = new System.Drawing.Point(0, 44);
            this.autoAddNewFilesCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.autoAddNewFilesCheckBox.Name = "autoAddNewFilesCheckBox";
            this.autoAddNewFilesCheckBox.Size = new System.Drawing.Size(213, 17);
            this.autoAddNewFilesCheckBox.TabIndex = 2;
            this.autoAddNewFilesCheckBox.Text = "Automatically add new files to repository";
            this.autoAddNewFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // tortoiseHgVersionTextBox
            // 
            this.tortoiseHgVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.SetColumnSpan(this.tortoiseHgVersionTextBox, 3);
            this.tortoiseHgVersionTextBox.Location = new System.Drawing.Point(0, 277);
            this.tortoiseHgVersionTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.tortoiseHgVersionTextBox.Name = "tortoiseHgVersionTextBox";
            this.tortoiseHgVersionTextBox.ReadOnly = true;
            this.tortoiseHgVersionTextBox.Size = new System.Drawing.Size(460, 20);
            this.tortoiseHgVersionTextBox.TabIndex = 14;
            this.tortoiseHgVersionTextBox.TabStop = false;
            // 
            // statusImageFileNameTextBox
            // 
            this.statusImageFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.statusImageFileNameTextBox.Location = new System.Drawing.Point(0, 227);
            this.statusImageFileNameTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.statusImageFileNameTextBox.Name = "statusImageFileNameTextBox";
            this.statusImageFileNameTextBox.Size = new System.Drawing.Size(423, 20);
            this.statusImageFileNameTextBox.TabIndex = 11;
            // 
            // selectStatusImageFileButton
            // 
            this.selectStatusImageFileButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.selectStatusImageFileButton.Location = new System.Drawing.Point(430, 224);
            this.selectStatusImageFileButton.Margin = new System.Windows.Forms.Padding(0);
            this.selectStatusImageFileButton.Name = "selectStatusImageFileButton";
            this.selectStatusImageFileButton.Size = new System.Drawing.Size(30, 26);
            this.selectStatusImageFileButton.TabIndex = 12;
            this.selectStatusImageFileButton.Text = "...";
            this.selectStatusImageFileButton.UseVisualStyleBackColor = true;
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
