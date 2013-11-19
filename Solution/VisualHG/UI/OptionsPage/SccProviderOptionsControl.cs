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
        private Button selectDiffToolTemplateButton;
        private TextBox diffToolTemplateTextBox;
        private TableLayoutPanel tableLayoutPanel1;
        private TextBox tortoiseHgVersionTextBox;
        private CheckBox enableContextSearchCheckBox;

        public Configuration Configuration
        {
            get
            {
                return new Configuration {
                    AutoActivatePlugin = autoActivateCheckBox.Checked,
                    AutoAddFiles = autoAddFilesCheckBox.Checked,
                    SearchIncludingChildren = enableContextSearchCheckBox.Checked,
                    ExternalDiffToolCommandMask = diffToolTemplateTextBox.Text,
                };
            }
            set
            {
                autoActivateCheckBox.Checked = value.AutoActivatePlugin;
                autoAddFilesCheckBox.Checked = value.AutoAddFiles;
                enableContextSearchCheckBox.Checked = value.SearchIncludingChildren;
                diffToolTemplateTextBox.Text = value.ExternalDiffToolCommandMask;
            }
        }

        public SccProviderOptionsControl()
        {
            InitializeComponent();

            tortoiseHgVersionTextBox.Text = TortoiseHg.Version ?? "TortoiseHg was not found";
        }

        private void ShowSelectDiffToolTemplateDialog(object sender, EventArgs e)
        {
            var selectDiffToolTemplateDialog = new SelectDiffToolTemplateDialog();
            var dialogResult = selectDiffToolTemplateDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                diffToolTemplateTextBox.Text = selectDiffToolTemplateDialog.SelectedTemplate;
            }
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
            System.Windows.Forms.Label diffToolTemplateLabel;
            System.Windows.Forms.Label tortoiseHgVersionLabel;
            System.Windows.Forms.Label noteLabel;
            this.autoAddFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.autoActivateCheckBox = new System.Windows.Forms.CheckBox();
            this.selectDiffToolTemplateButton = new System.Windows.Forms.Button();
            this.diffToolTemplateTextBox = new System.Windows.Forms.TextBox();
            this.enableContextSearchCheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tortoiseHgVersionTextBox = new System.Windows.Forms.TextBox();
            diffToolTemplateLabel = new System.Windows.Forms.Label();
            tortoiseHgVersionLabel = new System.Windows.Forms.Label();
            noteLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // diffToolTemplateLabel
            // 
            diffToolTemplateLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            diffToolTemplateLabel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(diffToolTemplateLabel, 2);
            diffToolTemplateLabel.Location = new System.Drawing.Point(0, 107);
            diffToolTemplateLabel.Margin = new System.Windows.Forms.Padding(0);
            diffToolTemplateLabel.Name = "diffToolTemplateLabel";
            diffToolTemplateLabel.Size = new System.Drawing.Size(134, 13);
            diffToolTemplateLabel.TabIndex = 0;
            diffToolTemplateLabel.Text = "External diff tool command:";
            // 
            // tortoiseHgVersionLabel
            // 
            tortoiseHgVersionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            tortoiseHgVersionLabel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(tortoiseHgVersionLabel, 2);
            tortoiseHgVersionLabel.Location = new System.Drawing.Point(0, 163);
            tortoiseHgVersionLabel.Margin = new System.Windows.Forms.Padding(0);
            tortoiseHgVersionLabel.Name = "tortoiseHgVersionLabel";
            tortoiseHgVersionLabel.Size = new System.Drawing.Size(99, 13);
            tortoiseHgVersionLabel.TabIndex = 0;
            tortoiseHgVersionLabel.Text = "TortoiseHg version:";
            // 
            // autoAddFilesCheckBox
            // 
            this.autoAddFilesCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoAddFilesCheckBox.AutoSize = true;
            this.autoAddFilesCheckBox.Checked = true;
            this.autoAddFilesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel1.SetColumnSpan(this.autoAddFilesCheckBox, 2);
            this.autoAddFilesCheckBox.Location = new System.Drawing.Point(0, 26);
            this.autoAddFilesCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.autoAddFilesCheckBox.Name = "autoAddFilesCheckBox";
            this.autoAddFilesCheckBox.Size = new System.Drawing.Size(247, 17);
            this.autoAddFilesCheckBox.TabIndex = 0;
            this.autoAddFilesCheckBox.Text = "Automatically add not tracked files to repository";
            this.autoAddFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoActivateCheckBox
            // 
            this.autoActivateCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoActivateCheckBox.AutoSize = true;
            this.autoActivateCheckBox.Checked = true;
            this.autoActivateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel1.SetColumnSpan(this.autoActivateCheckBox, 2);
            this.autoActivateCheckBox.Location = new System.Drawing.Point(0, 3);
            this.autoActivateCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.autoActivateCheckBox.Name = "autoActivateCheckBox";
            this.autoActivateCheckBox.Size = new System.Drawing.Size(269, 17);
            this.autoActivateCheckBox.TabIndex = 0;
            this.autoActivateCheckBox.Text = "Automatically activate plugin when opening solution";
            this.autoActivateCheckBox.UseVisualStyleBackColor = true;
            // 
            // selectDiffToolTemplateButton
            // 
            this.selectDiffToolTemplateButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.selectDiffToolTemplateButton.Location = new System.Drawing.Point(424, 126);
            this.selectDiffToolTemplateButton.Margin = new System.Windows.Forms.Padding(0);
            this.selectDiffToolTemplateButton.Name = "selectDiffToolTemplateButton";
            this.selectDiffToolTemplateButton.Size = new System.Drawing.Size(28, 20);
            this.selectDiffToolTemplateButton.TabIndex = 0;
            this.selectDiffToolTemplateButton.Text = "...";
            this.selectDiffToolTemplateButton.UseVisualStyleBackColor = true;
            this.selectDiffToolTemplateButton.Click += new System.EventHandler(this.ShowSelectDiffToolTemplateDialog);
            // 
            // diffToolTemplateTextBox
            // 
            this.diffToolTemplateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.diffToolTemplateTextBox.Location = new System.Drawing.Point(0, 126);
            this.diffToolTemplateTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.diffToolTemplateTextBox.Name = "diffToolTemplateTextBox";
            this.diffToolTemplateTextBox.Size = new System.Drawing.Size(418, 20);
            this.diffToolTemplateTextBox.TabIndex = 0;
            // 
            // enableContextSearchCheckBox
            // 
            this.enableContextSearchCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.enableContextSearchCheckBox.AutoSize = true;
            this.enableContextSearchCheckBox.Checked = true;
            this.enableContextSearchCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel1.SetColumnSpan(this.enableContextSearchCheckBox, 2);
            this.enableContextSearchCheckBox.Location = new System.Drawing.Point(0, 49);
            this.enableContextSearchCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.enableContextSearchCheckBox.Name = "enableContextSearchCheckBox";
            this.enableContextSearchCheckBox.Size = new System.Drawing.Size(359, 17);
            this.enableContextSearchCheckBox.TabIndex = 0;
            this.enableContextSearchCheckBox.Text = "Include child items for determining Add and Commit menu items visibility";
            this.enableContextSearchCheckBox.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tableLayoutPanel1.Controls.Add(this.autoActivateCheckBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.selectDiffToolTemplateButton, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.diffToolTemplateTextBox, 0, 6);
            this.tableLayoutPanel1.Controls.Add(diffToolTemplateLabel, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.enableContextSearchCheckBox, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.autoAddFilesCheckBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(tortoiseHgVersionLabel, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.tortoiseHgVersionTextBox, 0, 9);
            this.tableLayoutPanel1.Controls.Add(noteLabel, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 16;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(459, 334);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tortoiseHgVersionTextBox
            // 
            this.tortoiseHgVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tortoiseHgVersionTextBox, 2);
            this.tortoiseHgVersionTextBox.Location = new System.Drawing.Point(0, 182);
            this.tortoiseHgVersionTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.tortoiseHgVersionTextBox.Name = "tortoiseHgVersionTextBox";
            this.tortoiseHgVersionTextBox.ReadOnly = true;
            this.tortoiseHgVersionTextBox.Size = new System.Drawing.Size(459, 20);
            this.tortoiseHgVersionTextBox.TabIndex = 0;
            this.tortoiseHgVersionTextBox.TabStop = false;
            // 
            // noteLabel
            // 
            noteLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            noteLabel.AutoSize = true;
            noteLabel.Location = new System.Drawing.Point(16, 74);
            noteLabel.Margin = new System.Windows.Forms.Padding(16, 0, 0, 0);
            noteLabel.Name = "noteLabel";
            noteLabel.Size = new System.Drawing.Size(328, 13);
            noteLabel.TabIndex = 0;
            noteLabel.Text = "NOTE: This may slow down opening Solution Explorer context menu";
            // 
            // SccProviderOptionsControl
            // 
            this.AllowDrop = true;
            this.AutoScroll = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximumSize = new System.Drawing.Size(459, 334);
            this.MinimumSize = new System.Drawing.Size(459, 334);
            this.Name = "SccProviderOptionsControl";
            this.Size = new System.Drawing.Size(459, 334);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
