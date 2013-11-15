using System.Windows.Forms;

namespace VisualHg
{
    public partial class SelectDiffToolTemplateDialog : Form
    {
        public SelectDiffToolTemplateDialog()
        {
            ShowIcon = false;
            InitializeComponent();
        }

        public string SelectedTemplate
        {
            get{ return diffToolTemplateListCtrl.Text; }
        }
    }
}
