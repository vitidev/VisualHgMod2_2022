namespace VisualHG
{
    /// <summary>
    /// Command templates for external diff tools
    /// </summary>
    partial class SelectDiffToolTemplateDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.diffToolTemplateListCtrl = new System.Windows.Forms.ListBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // diffToolTemplateListCtrl
            // 
            this.diffToolTemplateListCtrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.diffToolTemplateListCtrl.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.diffToolTemplateListCtrl.FormattingEnabled = true;
            this.diffToolTemplateListCtrl.Items.AddRange(new object[] {
            "\"$(ProgramFiles)\\TortoiseSVN\\bin\\TortoiseMerge.exe\" /base:\"$(Base)\" /mine:\"$(Mine" +
                ")\" /basename:\"$(BaseName)\" /minename:\"$(MineName)\"",
            "\"$(ProgramFiles)\\Araxis\\Araxis Merge\\Compare.exe\" /wait /2 /title1:\"$(BaseName)\" " +
                "/title2:\"$(MineName)\" \"$(Base)\" \"$(Mine)\"",
            "\"$(ProgramFiles)\\Beyond Compare 3\\BComp.exe\" \"$(Base)\" \"$(Mine)\" /fv /title1=\"$(B" +
                "aseName)\" /title2=\"$(MineName)\"",
            "\"$(ProgramFiles)\\Compare It!\\wincmp.exe\" \"$(Base)\" \"/=$(BaseName)\" \"$(Mine)\" \"/=$" +
                "(MineName)\"",
            "\"$(ProgramFiles)\\Devart\\CodeCompare\\CodeComp.exe\" /WAIT /SC=HG /t1=\"$(BaseName)\" " +
                "/t2=\"$(MineName)\" \"$(Base)\" \"$(Mine)\"",
            "\"$(ProgramFiles)\\Ellié Computing\\Merge\\guimerge.exe\" \"$(Base)\" \"$(Mine)\" --mode=d" +
                "iff2 --title1=\"$(BaseName)\" --title2=\"$(MineName)\"",
            "\"$(ProgramFiles)\\TortoiseHG\\KDiff3.exe\" \"$(Base)\" --fname \"$(BaseName)\" \"$(Mine)\"" +
                " --fname \"$(MineName)\"",
            "\"$(ProgramFiles)\\Perforce\\p4merge.exe\" \"$(Base)\" \"$(Mine)\"",
            "\"$(ProgramFiles)\\ExamDiff\\ExamDiff.exe\" \"$(Base)\" \"$(Mine)\"",
            "\"$(ProgramFiles)\\SlickEdit\\win\\VSDiff.exe\" \"$(Base)\" \"$(Mine)\"",
            "\"$(ProgramFiles)\\SourceGear\\DiffMerge\\DiffMerge.exe\" \"$(Base)\" \"$(Mine)\" /t1=\"$(B" +
                "aseName)\" /t2=\"$(MineName)\"",
            "\"$(ProgramFiles)\\WinMerge\\WinMergeU.exe\" -e -x -u -wl -dl \"$(BaseName)\" -dr \"$(Mi" +
                "neName)\" \"$(base)\" \"$(mine)\""});
            this.diffToolTemplateListCtrl.Location = new System.Drawing.Point(12, 12);
            this.diffToolTemplateListCtrl.Name = "diffToolTemplateListCtrl";
            this.diffToolTemplateListCtrl.Size = new System.Drawing.Size(706, 238);
            this.diffToolTemplateListCtrl.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(643, 256);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(562, 256);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // SelectDiffToolTemplateDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(730, 301);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.diffToolTemplateListCtrl);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectDiffToolTemplateDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Tool Diff Template";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox diffToolTemplateListCtrl;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
    }
}