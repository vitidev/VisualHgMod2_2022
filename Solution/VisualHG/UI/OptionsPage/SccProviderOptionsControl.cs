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
        private Label label1;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
        private Button UpdateGlyphs;
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
            this.label1 = new System.Windows.Forms.Label();
            this.UpdateGlyphs = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "VisualHG options";
            // 
            // UpdateGlyphs
            // 
            this.UpdateGlyphs.Location = new System.Drawing.Point(126, 18);
            this.UpdateGlyphs.Name = "UpdateGlyphs";
            this.UpdateGlyphs.Size = new System.Drawing.Size(112, 23);
            this.UpdateGlyphs.TabIndex = 3;
            this.UpdateGlyphs.Text = "Update Glyphs";
            this.UpdateGlyphs.UseVisualStyleBackColor = true;
            this.UpdateGlyphs.Click += new System.EventHandler(this.UpdateGlyphs_Click);
            // 
            // SccProviderOptionsControl
            // 
            this.AllowDrop = true;
            this.Controls.Add(this.UpdateGlyphs);
            this.Controls.Add(this.label1);
            this.Name = "SccProviderOptionsControl";
            this.Size = new System.Drawing.Size(292, 195);
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

        private void UpdateGlyphs_Click(object sender, EventArgs e)
        {
            SccProviderService sccProviderService = (SccProviderService)GetService(typeof(SccProviderService));
            sccProviderService.RefreshNodesGlyphs();
        }
    }

}
