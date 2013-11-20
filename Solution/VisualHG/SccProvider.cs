using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    [InstalledProductRegistration(false, "#100", "#101", "1.1.5", IconResourceID = 400)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideMenuResource(1000, 1)]
    [ProvideOptionPage(typeof(SccProviderOptions), "Source Control", "VisualHg", 106, 107, false)]
    [ProvideOptionsPageVisibility("Source Control", "VisualHg", Guids.Provider)]
    [ProvideToolWindow(typeof(PendingChangesToolWindow))]
    [ProvideToolWindowVisibility(typeof(PendingChangesToolWindow), Guids.Provider)]
    [ProvideService(typeof(VisualHgService), ServiceName = "VisualHg")]
    [ProvideSourceControlProvider("VisualHg", "#100")]
    [ProvideAutoLoad(Guids.Provider)]
    [ProvideSolutionPersistence("VisualHgProperties")]
    [Guid(Guids.Package)]
    public sealed partial class SccProvider : Package, IOleCommandTarget
    {
        public string LastSeenProjectDirectory { get; set; }
        private IdlenessNotifier idlenessNotifier;
        private VisualHgService visualHgService;
        private PendingChangesToolWindow _pendingChangesToolWindow;
        
        private PendingChangesToolWindow PendingChangesToolWindow
        {
            get
            {
                if (_pendingChangesToolWindow == null)
                {
                    _pendingChangesToolWindow = FindToolWindow(typeof(PendingChangesToolWindow), 0, true) as PendingChangesToolWindow;

                    UpdatePendingChangesToolWindow();
                }

                return _pendingChangesToolWindow;
            }
        }


        public SccProvider()
        {
            Provider = this;
            LastSeenProjectDirectory = "";
            idlenessNotifier = new IdlenessNotifier();
        }


        private void NotifySolutionIsNotUnderVersionControl()
        {
            MessageBox.Show("Solution is not under Mercurial version contol\n\n" + SolutionFileName, "VisualHg", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void UpdatePendingChangesToolWindow()
        {
            var pendingFiles = visualHgService.Repository.PendingFiles;

            PendingChangesToolWindow.SetFiles(pendingFiles);
        }

        new public object GetService(Type serviceType)
        {
            return base.GetService(serviceType);
        }

        protected override void Initialize()
        {
            base.Initialize();

            visualHgService = new VisualHgService(this);
            ((IServiceContainer)this).AddService(typeof(VisualHgService), visualHgService, true);
            ((IServiceContainer)this).AddService(typeof(System.IServiceProvider), this, true);

            InitializeMenuCommands();

            var rscp = GetService(typeof(IVsRegisterScciProvider)) as IVsRegisterScciProvider;
            rscp.RegisterSourceControlProvider(Guids.ProviderGuid);

            idlenessNotifier.Register();
            idlenessNotifier.Idle += visualHgService.UpdateDirtyNodesGlyphs;
        }

        protected override void Dispose(bool disposing)
        {
            idlenessNotifier.Idle -= visualHgService.UpdateDirtyNodesGlyphs;
            idlenessNotifier.Revoke();

            Provider = null;

            visualHgService.Dispose();
            
            base.Dispose(disposing);
        }

        
        public static SccProvider Provider { get; private set; }
    }
}