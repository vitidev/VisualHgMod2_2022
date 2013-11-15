using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\9.0Exp")]
    [InstalledProductRegistration(false, "#100", "#101", "1.1.5", IconResourceID = CommandId.iiconProductIcon)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideMenuResource(1000, 1)]
    [ProvideOptionPage(typeof(SccProviderOptions), "Source Control", "VisualHg", 106, 107, false)]
    [ProvideOptionsPageVisibility("Source Control", "VisualHg", Guids.ProviderGuid)]
    [ProvideToolWindow(typeof(PendingChangesToolWindow))]
    [ProvideToolWindowVisibility(typeof(PendingChangesToolWindow), Guids.ProviderGuid)]
    [ProvideService(typeof(SccProviderService), ServiceName = "VisualHg")]
    [ProvideSourceControlProvider("VisualHg", "#100")]
    [ProvideAutoLoad(Guids.ProviderGuid)]
    [ProvideSolutionPersistence("VisualHgProperties")]
    [Guid(Guids.PackageGuid)]
    public sealed partial class SccProvider : Package, IOleCommandTarget
    {
        public string LastSeenProjectDirectory { get; set; }
        private IdleNotifier idleNotifier;
        private SccProviderService sccService;
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
            idleNotifier = new IdleNotifier();
        }


        private void NotifySolutionIsNotUnderVersionControl()
        {
            MessageBox.Show("Solution is not under Mercurial version contol\n\n" + SolutionFileName, "VisualHg", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void UpdatePendingChangesToolWindow()
        {
            var pendingFiles = sccService.Repository.GetPendingFiles();

            PendingChangesToolWindow.SetFiles(pendingFiles);
        }

        new public object GetService(Type serviceType)
        {
            return base.GetService(serviceType);
        }

        protected override void Initialize()
        {
            base.Initialize();

            sccService = new SccProviderService(this);
            ((IServiceContainer)this).AddService(typeof(SccProviderService), sccService, true);
            ((IServiceContainer)this).AddService(typeof(System.IServiceProvider), this, true);

            InitializeMenuCommands();

            var rscp = GetService(typeof(IVsRegisterScciProvider)) as IVsRegisterScciProvider;
            rscp.RegisterSourceControlProvider(Guids.guidSccProvider);

            idleNotifier.Register(GetGlobalService(typeof(SOleComponentManager)) as IOleComponentManager);
            idleNotifier.Idle += sccService.UpdateDirtyNodesGlyphs;
        }

        protected override void Dispose(bool disposing)
        {
            idleNotifier.Idle -= sccService.UpdateDirtyNodesGlyphs;
            idleNotifier.Revoke();

            Provider = null;

            sccService.Dispose();
            
            base.Dispose(disposing);
        }

        
        public static SccProvider Provider { get; private set; }
    }
}