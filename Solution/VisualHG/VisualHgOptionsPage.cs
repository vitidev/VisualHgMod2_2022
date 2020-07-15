using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using VisualHg.Controls;

namespace VisualHg
{
    [Guid(Guids.OptionsPage)]
    public class VisualHgOptionsPage : DialogPage
    {
        private readonly VisualHgOptionsControl control;

        protected override IWin32Window Window => control;

        public VisualHgOptionsPage()
        {
            control = new VisualHgOptionsControl();
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
            control.Configuration = VisualHgOptions.Global;
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            VisualHgOptions.Global = control.Configuration;
            base.OnApply(e);
        }
    }
}