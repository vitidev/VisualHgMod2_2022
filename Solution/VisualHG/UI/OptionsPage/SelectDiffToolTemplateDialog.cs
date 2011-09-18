using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VisualHG
{
    /// <summary>
    /// Command templates for external diff tools
    /// </summary>
    public partial class SelectDiffToolTemplateDialog : Form
    {
        public SelectDiffToolTemplateDialog()
        {
            ShowIcon = false;
            InitializeComponent();
        }

        public string selectedTemplate
        {
            get{ return diffToolTemplateListCtrl.Text; }
        }
    }
}
