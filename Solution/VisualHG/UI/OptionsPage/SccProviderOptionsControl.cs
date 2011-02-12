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

namespace VisualHG
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
            this.autoActivatePlugin.Text = "Autoselect VisualHG for Mercurial solutions";
            this.autoActivatePlugin.UseVisualStyleBackColor = true;
            // 
            // SccProviderOptionsControl
            // 
            this.AllowDrop = true;
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
            config._autoActivatePlugin = autoActivatePlugin.Checked;
            config._autoAddFiles       = autoAddFiles.Checked;
        }

        public void RestoreConfiguration(Configuration config)
        {
            autoActivatePlugin.Checked = config._autoActivatePlugin;
            autoAddFiles.Checked = config._autoAddFiles;
        }

        private void UpdateGlyphs_Click(object sender, EventArgs e)
        {
            SccProviderService sccProviderService = (SccProviderService)GetService(typeof(SccProviderService));
            sccProviderService.RefreshNodesGlyphs();
        }
    }

}
