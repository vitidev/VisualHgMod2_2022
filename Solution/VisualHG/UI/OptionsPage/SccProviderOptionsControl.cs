using System;
using System.ComponentModel;
using System.Windows.Forms;
using HgLib;

namespace VisualHg
{
    public class SccProviderOptionsControl : UserControl
    {
        private Container components;
        private CheckBox addFilesOnLoadCheckBox;
        private CheckBox autoActivateCheckBox;
        private Button selectDiffToolButton;
        private TextBox diffToolPathTextBox;
        private TableLayoutPanel tableLayoutPanel;
        private TextBox tortoiseHgVersionTextBox;
        private TextBox diffToolArgumentsTextBox;
        private OpenFileDialog openFileDialog;
        private CheckBox autoAddNewFilesCheckBox;
        private CheckBox enableContextSearchCheckBox;

        public Configuration Configuration
        {
            get
            {
                return new Configuration {
                    AutoActivatePlugin = autoActivateCheckBox.Checked,
                    AddFilesOnLoad = addFilesOnLoadCheckBox.Checked,
                    AutoAddNewFiles = autoAddNewFilesCheckBox.Checked,
                    SearchIncludingChildren = enableContextSearchCheckBox.Checked,
                    DiffToolPath = diffToolPathTextBox.Text,
                    DiffToolArguments = diffToolArgumentsTextBox.Text,
                };
            }
            set
            {
                autoActivateCheckBox.Checked = value.AutoActivatePlugin;
                addFilesOnLoadCheckBox.Checked = value.AddFilesOnLoad;
                autoAddNewFilesCheckBox.Checked = value.AutoAddNewFiles;
                enableContextSearchCheckBox.Checked = value.SearchIncludingChildren;
                diffToolPathTextBox.Text = value.DiffToolPath;
                diffToolArgumentsTextBox.Text = value.DiffToolArguments;
            }
        }

        public SccProviderOptionsControl()
        {
            InitializeComponent();

            tortoiseHgVersionTextBox.Text = TortoiseHg.Version ?? "TortoiseHg was not found";

            selectDiffToolButton.Click += SelectDiffTool;
            openFileDialog.FileOk += (s, e) => diffToolPathTextBox.Text = openFileDialog.FileName;
        }

        private void SelectDiffTool(object sender, EventArgs e)
        {
            openFileDialog.FileName = diffToolPathTextBox.Text;
            openFileDialog.ShowDialog();
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
            this.addFilesOnLoadCheckBox = new System.Windows.Forms.CheckBox();
            this.autoActivateCheckBox = new System.Windows.Forms.CheckBox();
            this.selectDiffToolButton = new System.Windows.Forms.Button();
            this.diffToolPathTextBox = new System.Windows.Forms.TextBox();
            this.enableContextSearchCheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tortoiseHgVersionTextBox = new System.Windows.Forms.TextBox();
            this.diffToolArgumentsTextBox = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.autoAddNewFilesCheckBox = new System.Windows.Forms.CheckBox();
            diffToolPathLabel = new System.Windows.Forms.Label();
            tortoiseHgVersionLabel = new System.Windows.Forms.Label();
            noteLabel = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // diffToolPathLabel
            // 
            diffToolPathLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            diffToolPathLabel.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(diffToolPathLabel, 2);
            diffToolPathLabel.Location = new System.Drawing.Point(0, 130);
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
            this.tableLayoutPanel.SetColumnSpan(tortoiseHgVersionLabel, 2);
            tortoiseHgVersionLabel.Location = new System.Drawing.Point(0, 232);
            tortoiseHgVersionLabel.Margin = new System.Windows.Forms.Padding(0);
            tortoiseHgVersionLabel.Name = "tortoiseHgVersionLabel";
            tortoiseHgVersionLabel.Size = new System.Drawing.Size(99, 13);
            tortoiseHgVersionLabel.TabIndex = 10;
            tortoiseHgVersionLabel.Text = "TortoiseHg version:";
            // 
            // noteLabel
            // 
            noteLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            noteLabel.AutoSize = true;
            noteLabel.Location = new System.Drawing.Point(16, 97);
            noteLabel.Margin = new System.Windows.Forms.Padding(16, 0, 0, 0);
            noteLabel.Name = "noteLabel";
            noteLabel.Size = new System.Drawing.Size(328, 13);
            noteLabel.TabIndex = 4;
            noteLabel.Text = "NOTE: This may slow down opening Solution Explorer context menu";
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(label1, 2);
            label1.Location = new System.Drawing.Point(0, 176);
            label1.Margin = new System.Windows.Forms.Padding(0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(429, 13);
            label1.TabIndex = 8;
            label1.Text = "Diff tool arguments (use %PathA%, %NameA%, %PathB%, %NameB%, no quotes needed):";
            // 
            // addFilesOnLoadCheckBox
            // 
            this.addFilesOnLoadCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.addFilesOnLoadCheckBox.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.addFilesOnLoadCheckBox, 2);
            this.addFilesOnLoadCheckBox.Location = new System.Drawing.Point(0, 26);
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
            this.tableLayoutPanel.SetColumnSpan(this.autoActivateCheckBox, 2);
            this.autoActivateCheckBox.Location = new System.Drawing.Point(0, 3);
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
            this.selectDiffToolButton.Location = new System.Drawing.Point(419, 148);
            this.selectDiffToolButton.Margin = new System.Windows.Forms.Padding(0);
            this.selectDiffToolButton.Name = "selectDiffToolButton";
            this.selectDiffToolButton.Size = new System.Drawing.Size(40, 23);
            this.selectDiffToolButton.TabIndex = 7;
            this.selectDiffToolButton.Text = "...";
            this.selectDiffToolButton.UseVisualStyleBackColor = true;
            // 
            // diffToolPathTextBox
            // 
            this.diffToolPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.diffToolPathTextBox.Location = new System.Drawing.Point(0, 149);
            this.diffToolPathTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.diffToolPathTextBox.Name = "diffToolPathTextBox";
            this.diffToolPathTextBox.Size = new System.Drawing.Size(419, 20);
            this.diffToolPathTextBox.TabIndex = 6;
            // 
            // enableContextSearchCheckBox
            // 
            this.enableContextSearchCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.enableContextSearchCheckBox.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.enableContextSearchCheckBox, 2);
            this.enableContextSearchCheckBox.Location = new System.Drawing.Point(0, 72);
            this.enableContextSearchCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.enableContextSearchCheckBox.Name = "enableContextSearchCheckBox";
            this.enableContextSearchCheckBox.Size = new System.Drawing.Size(359, 17);
            this.enableContextSearchCheckBox.TabIndex = 3;
            this.enableContextSearchCheckBox.Text = "Include child items for determining Add and Commit menu items visibility";
            this.enableContextSearchCheckBox.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Controls.Add(this.autoActivateCheckBox, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.selectDiffToolButton, 1, 7);
            this.tableLayoutPanel.Controls.Add(this.diffToolPathTextBox, 0, 7);
            this.tableLayoutPanel.Controls.Add(diffToolPathLabel, 0, 6);
            this.tableLayoutPanel.Controls.Add(this.enableContextSearchCheckBox, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.addFilesOnLoadCheckBox, 0, 1);
            this.tableLayoutPanel.Controls.Add(noteLabel, 0, 4);
            this.tableLayoutPanel.Controls.Add(this.tortoiseHgVersionTextBox, 0, 12);
            this.tableLayoutPanel.Controls.Add(tortoiseHgVersionLabel, 0, 11);
            this.tableLayoutPanel.Controls.Add(label1, 0, 8);
            this.tableLayoutPanel.Controls.Add(this.diffToolArgumentsTextBox, 0, 9);
            this.tableLayoutPanel.Controls.Add(this.autoAddNewFilesCheckBox, 0, 2);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 16;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(459, 334);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // tortoiseHgVersionTextBox
            // 
            this.tortoiseHgVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.SetColumnSpan(this.tortoiseHgVersionTextBox, 2);
            this.tortoiseHgVersionTextBox.Location = new System.Drawing.Point(0, 251);
            this.tortoiseHgVersionTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.tortoiseHgVersionTextBox.Name = "tortoiseHgVersionTextBox";
            this.tortoiseHgVersionTextBox.ReadOnly = true;
            this.tortoiseHgVersionTextBox.Size = new System.Drawing.Size(459, 20);
            this.tortoiseHgVersionTextBox.TabIndex = 11;
            this.tortoiseHgVersionTextBox.TabStop = false;
            // 
            // diffToolArgumentsTextBox
            // 
            this.diffToolArgumentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.SetColumnSpan(this.diffToolArgumentsTextBox, 2);
            this.diffToolArgumentsTextBox.Location = new System.Drawing.Point(0, 195);
            this.diffToolArgumentsTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.diffToolArgumentsTextBox.Name = "diffToolArgumentsTextBox";
            this.diffToolArgumentsTextBox.Size = new System.Drawing.Size(459, 20);
            this.diffToolArgumentsTextBox.TabIndex = 9;
            // 
            // openFileDialog
            // 
            this.openFileDialog.AddExtension = false;
            this.openFileDialog.DefaultExt = "exe";
            this.openFileDialog.Filter = "Executable files|*.exe|All files|*.*";
            this.openFileDialog.ShowReadOnly = true;
            this.openFileDialog.Title = "Select diff tool";
            // 
            // autoAddNewFilesCheckBox
            // 
            this.autoAddNewFilesCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoAddNewFilesCheckBox.AutoSize = true;
            this.autoAddNewFilesCheckBox.Location = new System.Drawing.Point(0, 49);
            this.autoAddNewFilesCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.autoAddNewFilesCheckBox.Name = "autoAddNewFilesCheckBox";
            this.autoAddNewFilesCheckBox.Size = new System.Drawing.Size(213, 17);
            this.autoAddNewFilesCheckBox.TabIndex = 2;
            this.autoAddNewFilesCheckBox.Text = "Automatically add new files to repository";
            this.autoAddNewFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // SccProviderOptionsControl
            // 
            this.AllowDrop = true;
            this.AutoScroll = true;
            this.Controls.Add(this.tableLayoutPanel);
            this.MaximumSize = new System.Drawing.Size(459, 334);
            this.MinimumSize = new System.Drawing.Size(459, 334);
            this.Name = "SccProviderOptionsControl";
            this.Size = new System.Drawing.Size(459, 334);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
