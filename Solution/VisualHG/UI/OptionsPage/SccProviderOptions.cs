using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace VisualHg
{
    [Guid(Guids.ProviderOptionsPageGuid)]
    public class SccProviderOptions : DialogPage
    {
        private SccProviderOptionsControl _optionsControl;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get { return _optionsControl; }
        }

        public SccProviderOptions()
        {
            _optionsControl = new SccProviderOptionsControl();
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
         
            _optionsControl.Configuration = Configuration.Global;
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
            
            Configuration.Global = _optionsControl.Configuration;
        }
    }
}