using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace VisualHg
{
    [Guid(Guids.OptionsPage)]
    public class SccProviderOptions : DialogPage
    {
        private SccProviderOptionsControl control;

        protected override IWin32Window Window
        {
            get { return control; }
        }

        public SccProviderOptions()
        {
            control = new SccProviderOptionsControl();
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
            control.Configuration = Configuration.Global;
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            Configuration.Global = control.Configuration;
            base.OnApply(e);
        }
    }
}