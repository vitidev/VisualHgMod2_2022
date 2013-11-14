using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
	/// <summary>
    /// Summary description for SccProviderOptionsControl.
	/// </summary>
	public class SccProviderOptionsControl : System.Windows.Forms.UserControl
    {

		/// <summary> 
		/// Required designer variable.
		/// </summary>
        private System.ComponentModel.Container components = null;
        private CheckBox autoAddFiles;
        private CheckBox autoActivatePlugin;
        private Button editDiffToolButton;
        private TextBox externalDiffToolCommandEdit;
        private Label label1;
        private CheckBox observeOutOfStudioFileChanges;
        private CheckBox enableContextSearch;
        // The parent page, use to persist data
        private SccProviderOptions _customPage;

        public SccProviderOptionsControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
				GC.SuppressFinalize(this);
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.autoAddFiles = new System.Windows.Forms.CheckBox();
            this.autoActivatePlugin = new System.Windows.Forms.CheckBox();
            this.editDiffToolButton = new System.Windows.Forms.Button();
            this.externalDiffToolCommandEdit = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.observeOutOfStudioFileChanges = new System.Windows.Forms.CheckBox();
            this.enableContextSearch = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // autoAddFiles
            // 
            this.autoAddFiles.AutoSize = true;
            this.autoAddFiles.Checked = true;
            this.autoAddFiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoAddFiles.Location = new System.Drawing.Point(3, 26);
            this.autoAddFiles.Name = "autoAddFiles";
            this.autoAddFiles.Size = new System.Drawing.Size(299, 17);
            this.autoAddFiles.TabIndex = 0;
            this.autoAddFiles.Text = "Add files automatically to Mercurial ( except ignored ones )";
            this.autoAddFiles.UseVisualStyleBackColor = true;
            // 
            // autoActivatePlugin
            // 
            this.autoActivatePlugin.AutoSize = true;
            this.autoActivatePlugin.Checked = true;
            this.autoActivatePlugin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoActivatePlugin.Location = new System.Drawing.Point(3, 3);
            this.autoActivatePlugin.Name = "autoActivatePlugin";
            this.autoActivatePlugin.Size = new System.Drawing.Size(228, 17);
            this.autoActivatePlugin.TabIndex = 1;
            this.autoActivatePlugin.Text = "Autoselect VisualHg for Mercurial solutions";
            this.autoActivatePlugin.UseVisualStyleBackColor = true;
            // 
            // editDiffToolButton
            // 
            this.editDiffToolButton.Location = new System.Drawing.Point(369, 116);
            this.editDiffToolButton.Name = "editDiffToolButton";
            this.editDiffToolButton.Size = new System.Drawing.Size(28, 23);
            this.editDiffToolButton.TabIndex = 2;
            this.editDiffToolButton.Text = "...";
            this.editDiffToolButton.UseVisualStyleBackColor = true;
            this.editDiffToolButton.Click += new System.EventHandler(this.OnEditDiffToolButton);
            // 
            // externalDiffToolCommandEdit
            // 
            this.externalDiffToolCommandEdit.Location = new System.Drawing.Point(3, 118);
            this.externalDiffToolCommandEdit.Name = "externalDiffToolCommandEdit";
            this.externalDiffToolCommandEdit.Size = new System.Drawing.Size(360, 20);
            this.externalDiffToolCommandEdit.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "External Diff Tool Command";
            // 
            // observeOutOfStudioFileChanges
            // 
            this.observeOutOfStudioFileChanges.AutoSize = true;
            this.observeOutOfStudioFileChanges.Checked = true;
            this.observeOutOfStudioFileChanges.CheckState = System.Windows.Forms.CheckState.Checked;
            this.observeOutOfStudioFileChanges.Location = new System.Drawing.Point(3, 49);
            this.observeOutOfStudioFileChanges.Name = "observeOutOfStudioFileChanges";
            this.observeOutOfStudioFileChanges.Size = new System.Drawing.Size(189, 17);
            this.observeOutOfStudioFileChanges.TabIndex = 5;
            this.observeOutOfStudioFileChanges.Text = "Observe out of Studio file changes";
            this.observeOutOfStudioFileChanges.UseVisualStyleBackColor = true;
            // 
            // enableContextSearch
            // 
            this.enableContextSearch.AutoSize = true;
            this.enableContextSearch.Checked = true;
            this.enableContextSearch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableContextSearch.Location = new System.Drawing.Point(3, 72);
            this.enableContextSearch.Name = "enableContextSearch";
            this.enableContextSearch.Size = new System.Drawing.Size(379, 17);
            this.enableContextSearch.TabIndex = 6;
            this.enableContextSearch.Text = "Context sensitive Add and Commit Menu (can become slow on huge repos)";
            this.enableContextSearch.UseVisualStyleBackColor = true;
            // 
            // SccProviderOptionsControl
            // 
            this.AllowDrop = true;
            this.Controls.Add(this.enableContextSearch);
            this.Controls.Add(this.observeOutOfStudioFileChanges);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.externalDiffToolCommandEdit);
            this.Controls.Add(this.editDiffToolButton);
            this.Controls.Add(this.autoActivatePlugin);
            this.Controls.Add(this.autoAddFiles);
            this.Name = "SccProviderOptionsControl";
            this.Size = new System.Drawing.Size(400, 271);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
    
        public SccProviderOptions OptionsPage
        {
            set
            {
                _customPage = value;
            }
        }

        public void StoreConfiguration(Configuration config)
        {
            config.AutoActivatePlugin               = autoActivatePlugin.Checked;
            config.AutoAddFiles                     = autoAddFiles.Checked;
            config.EnableContextSearch              = enableContextSearch.Checked;
            config.ObserveOutOfStudioFileChanges    = observeOutOfStudioFileChanges.Checked;
            config.ExternalDiffToolCommandMask      = externalDiffToolCommandEdit.Text;
        }

        public void RestoreConfiguration(Configuration config)
        {
            autoActivatePlugin.Checked = config.AutoActivatePlugin;
            autoAddFiles.Checked = config.AutoAddFiles;
            enableContextSearch.Checked = config.EnableContextSearch;
            observeOutOfStudioFileChanges.Checked = config.ObserveOutOfStudioFileChanges;
            externalDiffToolCommandEdit.Text = config.ExternalDiffToolCommandMask;
        }

        private void OnEditDiffToolButton(object sender, EventArgs e)
        {
            SelectDiffToolTemplateDialog selectDiffToolTemplateDialog = new SelectDiffToolTemplateDialog();
            DialogResult result = selectDiffToolTemplateDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                externalDiffToolCommandEdit.Text = selectDiffToolTemplateDialog.selectedTemplate;
            }
        }
    }
}
