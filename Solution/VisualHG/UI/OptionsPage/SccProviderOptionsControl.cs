using System;
using System.ComponentModel;
using System.Windows.Forms;
using HgLib;

namespace VisualHg
{
    public class SccProviderOptionsControl : UserControl
    {
        private Container components;
        private CheckBox autoAddFilesCheckBox;
        private CheckBox autoActivateCheckBox;
        private Button selectDiffToolButton;
        private TextBox diffToolPathTextBox;
        private TableLayoutPanel tableLayoutPanel;
        private TextBox tortoiseHgVersionTextBox;
        private TextBox diffToolArgumentsTextBox;
        private OpenFileDialog openFileDialog;
        private CheckBox enableContextSearchCheckBox;

        public Configuration Configuration
        {
            get
            {
                return new Configuration {
                    AutoActivatePlugin = autoActivateCheckBox.Checked,
                    AutoAddFiles = autoAddFilesCheckBox.Checked,
                    SearchIncludingChildren = enableContextSearchCheckBox.Checked,
                    DiffToolPath = diffToolPathTextBox.Text,
                    DiffToolArguments = diffToolArgumentsTextBox.Text,
                };
            }
            set
            {
                autoActivateCheckBox.Checked = value.AutoActivatePlugin;
                autoAddFilesCheckBox.Checked = value.AutoAddFiles;
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
            this.autoAddFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.autoActivateCheckBox = new System.Windows.Forms.CheckBox();
            this.selectDiffToolButton = new System.Windows.Forms.Button();
            this.diffToolPathTextBox = new System.Windows.Forms.TextBox();
            this.enableContextSearchCheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tortoiseHgVersionTextBox = new System.Windows.Forms.TextBox();
            this.diffToolArgumentsTextBox = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
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
            diffToolPathLabel.Location = new System.Drawing.Point(0, 107);
            diffToolPathLabel.Margin = new System.Windows.Forms.Padding(0);
            diffToolPathLabel.Name = "diffToolPathLabel";
            diffToolPathLabel.Size = new System.Drawing.Size(207, 13);
            diffToolPathLabel.TabIndex = 4;
            diffToolPathLabel.Text = "Custom diff tool (leave blank to use kdiff3):";
            // 
            // tortoiseHgVersionLabel
            // 
            tortoiseHgVersionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            tortoiseHgVersionLabel.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(tortoiseHgVersionLabel, 2);
            tortoiseHgVersionLabel.Location = new System.Drawing.Point(0, 209);
            tortoiseHgVersionLabel.Margin = new System.Windows.Forms.Padding(0);
            tortoiseHgVersionLabel.Name = "tortoiseHgVersionLabel";
            tortoiseHgVersionLabel.Size = new System.Drawing.Size(99, 13);
            tortoiseHgVersionLabel.TabIndex = 9;
            tortoiseHgVersionLabel.Text = "TortoiseHg version:";
            // 
            // noteLabel
            // 
            noteLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            noteLabel.AutoSize = true;
            noteLabel.Location = new System.Drawing.Point(16, 74);
            noteLabel.Margin = new System.Windows.Forms.Padding(16, 0, 0, 0);
            noteLabel.Name = "noteLabel";
            noteLabel.Size = new System.Drawing.Size(328, 13);
            noteLabel.TabIndex = 3;
            noteLabel.Text = "NOTE: This may slow down opening Solution Explorer context menu";
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(0, 153);
            label1.Margin = new System.Windows.Forms.Padding(0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(337, 13);
            label1.TabIndex = 7;
            label1.Text = "Diff tool arguments (use %PathA%, %NameA%, %PathB%, %NameB%):";
            // 
            // autoAddFilesCheckBox
            // 
            this.autoAddFilesCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoAddFilesCheckBox.AutoSize = true;
            this.autoAddFilesCheckBox.Checked = true;
            this.autoAddFilesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel.SetColumnSpan(this.autoAddFilesCheckBox, 2);
            this.autoAddFilesCheckBox.Location = new System.Drawing.Point(0, 26);
            this.autoAddFilesCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.autoAddFilesCheckBox.Name = "autoAddFilesCheckBox";
            this.autoAddFilesCheckBox.Size = new System.Drawing.Size(247, 17);
            this.autoAddFilesCheckBox.TabIndex = 1;
            this.autoAddFilesCheckBox.Text = "Automatically add not tracked files to repository";
            this.autoAddFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoActivateCheckBox
            // 
            this.autoActivateCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoActivateCheckBox.AutoSize = true;
            this.autoActivateCheckBox.Checked = true;
            this.autoActivateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel.SetColumnSpan(this.autoActivateCheckBox, 2);
            this.autoActivateCheckBox.Location = new System.Drawing.Point(0, 3);
            this.autoActivateCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.autoActivateCheckBox.Name = "autoActivateCheckBox";
            this.autoActivateCheckBox.Size = new System.Drawing.Size(269, 17);
            this.autoActivateCheckBox.TabIndex = 0;
            this.autoActivateCheckBox.Text = "Automatically activate plugin when opening solution";
            this.autoActivateCheckBox.UseVisualStyleBackColor = true;
            // 
            // selectDiffToolButton
            // 
            this.selectDiffToolButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.selectDiffToolButton.Location = new System.Drawing.Point(419, 125);
            this.selectDiffToolButton.Margin = new System.Windows.Forms.Padding(0);
            this.selectDiffToolButton.Name = "selectDiffToolButton";
            this.selectDiffToolButton.Size = new System.Drawing.Size(40, 23);
            this.selectDiffToolButton.TabIndex = 6;
            this.selectDiffToolButton.Text = "...";
            this.selectDiffToolButton.UseVisualStyleBackColor = true;
            // 
            // diffToolPathTextBox
            // 
            this.diffToolPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.diffToolPathTextBox.Location = new System.Drawing.Point(0, 126);
            this.diffToolPathTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.diffToolPathTextBox.Name = "diffToolPathTextBox";
            this.diffToolPathTextBox.Size = new System.Drawing.Size(419, 20);
            this.diffToolPathTextBox.TabIndex = 5;
            // 
            // enableContextSearchCheckBox
            // 
            this.enableContextSearchCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.enableContextSearchCheckBox.AutoSize = true;
            this.enableContextSearchCheckBox.Checked = true;
            this.enableContextSearchCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel.SetColumnSpan(this.enableContextSearchCheckBox, 2);
            this.enableContextSearchCheckBox.Location = new System.Drawing.Point(0, 49);
            this.enableContextSearchCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.enableContextSearchCheckBox.Name = "enableContextSearchCheckBox";
            this.enableContextSearchCheckBox.Size = new System.Drawing.Size(359, 17);
            this.enableContextSearchCheckBox.TabIndex = 2;
            this.enableContextSearchCheckBox.Text = "Include child items for determining Add and Commit menu items visibility";
            this.enableContextSearchCheckBox.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Controls.Add(this.autoActivateCheckBox, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.selectDiffToolButton, 1, 6);
            this.tableLayoutPanel.Controls.Add(this.diffToolPathTextBox, 0, 6);
            this.tableLayoutPanel.Controls.Add(diffToolPathLabel, 0, 5);
            this.tableLayoutPanel.Controls.Add(this.enableContextSearchCheckBox, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.autoAddFilesCheckBox, 0, 1);
            this.tableLayoutPanel.Controls.Add(noteLabel, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.tortoiseHgVersionTextBox, 0, 11);
            this.tableLayoutPanel.Controls.Add(tortoiseHgVersionLabel, 0, 10);
            this.tableLayoutPanel.Controls.Add(label1, 0, 7);
            this.tableLayoutPanel.Controls.Add(this.diffToolArgumentsTextBox, 0, 8);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 16;
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
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(459, 334);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // tortoiseHgVersionTextBox
            // 
            this.tortoiseHgVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.SetColumnSpan(this.tortoiseHgVersionTextBox, 2);
            this.tortoiseHgVersionTextBox.Location = new System.Drawing.Point(0, 228);
            this.tortoiseHgVersionTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.tortoiseHgVersionTextBox.Name = "tortoiseHgVersionTextBox";
            this.tortoiseHgVersionTextBox.ReadOnly = true;
            this.tortoiseHgVersionTextBox.Size = new System.Drawing.Size(459, 20);
            this.tortoiseHgVersionTextBox.TabIndex = 10;
            this.tortoiseHgVersionTextBox.TabStop = false;
            // 
            // diffToolArgumentsTextBox
            // 
            this.diffToolArgumentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.SetColumnSpan(this.diffToolArgumentsTextBox, 2);
            this.diffToolArgumentsTextBox.Location = new System.Drawing.Point(0, 172);
            this.diffToolArgumentsTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.diffToolArgumentsTextBox.Name = "diffToolArgumentsTextBox";
            this.diffToolArgumentsTextBox.Size = new System.Drawing.Size(459, 20);
            this.diffToolArgumentsTextBox.TabIndex = 8;
            // 
            // openFileDialog
            // 
            this.openFileDialog.AddExtension = false;
            this.openFileDialog.DefaultExt = "exe";
            this.openFileDialog.Filter = "Executable files|*.exe|All files|*.*";
            this.openFileDialog.ShowReadOnly = true;
            this.openFileDialog.Title = "Select diff tool";
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
