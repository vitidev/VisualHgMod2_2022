using System;
using System.Windows.Forms;

namespace VisualHg
{
    public class SccProviderOptionsControl : UserControl
    {
        private System.ComponentModel.Container components = null;
        private CheckBox autoAddFilesCheckBox;
        private CheckBox autoActivateCheckBox;
        private Button selectDiffToolTemplateButton;
        private TextBox diffToolTemplateTextBox;
        private CheckBox enableContextSearchCheckBox;

        public Configuration Configuration
        {
            get
            {
                return new Configuration {
                    AutoActivatePlugin = autoActivateCheckBox.Checked,
                    AutoAddFiles = autoAddFilesCheckBox.Checked,
                    EnableContextSearch = enableContextSearchCheckBox.Checked,
                    ExternalDiffToolCommandMask = diffToolTemplateTextBox.Text,
                };
            }
            set
            {
                autoActivateCheckBox.Checked = value.AutoActivatePlugin;
                autoAddFilesCheckBox.Checked = value.AutoAddFiles;
                enableContextSearchCheckBox.Checked = value.EnableContextSearch;
                diffToolTemplateTextBox.Text = value.ExternalDiffToolCommandMask;
            }
        }

        public SccProviderOptionsControl()
        {
            InitializeComponent();
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
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label diffToolTemplateLabel;
            this.autoAddFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.autoActivateCheckBox = new System.Windows.Forms.CheckBox();
            this.selectDiffToolTemplateButton = new System.Windows.Forms.Button();
            this.diffToolTemplateTextBox = new System.Windows.Forms.TextBox();
            this.enableContextSearchCheckBox = new System.Windows.Forms.CheckBox();
            diffToolTemplateLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // autoAddFilesCheckBox
            // 
            this.autoAddFilesCheckBox.AutoSize = true;
            this.autoAddFilesCheckBox.Checked = true;
            this.autoAddFilesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoAddFilesCheckBox.Location = new System.Drawing.Point(3, 26);
            this.autoAddFilesCheckBox.Name = "autoAddFilesCheckBox";
            this.autoAddFilesCheckBox.Size = new System.Drawing.Size(293, 17);
            this.autoAddFilesCheckBox.TabIndex = 0;
            this.autoAddFilesCheckBox.Text = "Add files automatically to Mercurial (except ignored ones)";
            this.autoAddFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoActivateCheckBox
            // 
            this.autoActivateCheckBox.AutoSize = true;
            this.autoActivateCheckBox.Checked = true;
            this.autoActivateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoActivateCheckBox.Location = new System.Drawing.Point(3, 3);
            this.autoActivateCheckBox.Name = "autoActivateCheckBox";
            this.autoActivateCheckBox.Size = new System.Drawing.Size(226, 17);
            this.autoActivateCheckBox.TabIndex = 1;
            this.autoActivateCheckBox.Text = "Autoselect VisualHg for Mercurial solutions";
            this.autoActivateCheckBox.UseVisualStyleBackColor = true;
            // 
            // selectDiffToolTemplateButton
            // 
            this.selectDiffToolTemplateButton.Location = new System.Drawing.Point(369, 98);
            this.selectDiffToolTemplateButton.Name = "selectDiffToolTemplateButton";
            this.selectDiffToolTemplateButton.Size = new System.Drawing.Size(28, 23);
            this.selectDiffToolTemplateButton.TabIndex = 2;
            this.selectDiffToolTemplateButton.Text = "...";
            this.selectDiffToolTemplateButton.UseVisualStyleBackColor = true;
            this.selectDiffToolTemplateButton.Click += new System.EventHandler(this.ShowSelectDiffToolTemplateDialog);
            // 
            // diffToolTemplateTextBox
            // 
            this.diffToolTemplateTextBox.Location = new System.Drawing.Point(3, 100);
            this.diffToolTemplateTextBox.Name = "diffToolTemplateTextBox";
            this.diffToolTemplateTextBox.Size = new System.Drawing.Size(360, 20);
            this.diffToolTemplateTextBox.TabIndex = 3;
            // 
            // diffToolTemplateLabel
            // 
            diffToolTemplateLabel.AutoSize = true;
            diffToolTemplateLabel.Location = new System.Drawing.Point(3, 84);
            diffToolTemplateLabel.Name = "diffToolTemplateLabel";
            diffToolTemplateLabel.Size = new System.Drawing.Size(138, 13);
            diffToolTemplateLabel.TabIndex = 4;
            diffToolTemplateLabel.Text = "External Diff Tool Command";
            // 
            // enableContextSearchCheckBox
            // 
            this.enableContextSearchCheckBox.AutoSize = true;
            this.enableContextSearchCheckBox.Checked = true;
            this.enableContextSearchCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableContextSearchCheckBox.Location = new System.Drawing.Point(3, 49);
            this.enableContextSearchCheckBox.Name = "enableContextSearchCheckBox";
            this.enableContextSearchCheckBox.Size = new System.Drawing.Size(379, 17);
            this.enableContextSearchCheckBox.TabIndex = 6;
            this.enableContextSearchCheckBox.Text = "Context sensitive Add and Commit Menu (can become slow on huge repos)";
            this.enableContextSearchCheckBox.UseVisualStyleBackColor = true;
            // 
            // SccProviderOptionsControl
            // 
            this.AllowDrop = true;
            this.Controls.Add(this.enableContextSearchCheckBox);
            this.Controls.Add(diffToolTemplateLabel);
            this.Controls.Add(this.diffToolTemplateTextBox);
            this.Controls.Add(this.selectDiffToolTemplateButton);
            this.Controls.Add(this.autoActivateCheckBox);
            this.Controls.Add(this.autoAddFilesCheckBox);
            this.Name = "SccProviderOptionsControl";
            this.Size = new System.Drawing.Size(400, 271);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

    }
}
