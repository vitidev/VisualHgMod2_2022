using System;
using System.IO;
using System.Resources;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using System.Windows.Forms;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;
using System.Runtime.Serialization.Formatters.Binary;
using MsVsShell = Microsoft.VisualStudio.Shell;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace VisualHg
{
    /////////////////////////////////////////////////////////////////////////////
    // SccProvider
    [MsVsShell.DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\9.0Exp")]
    // Register the package to have information displayed in Help/About dialog box
    [MsVsShell.InstalledProductRegistration(false, "#100", "#101", "1.1.5", IconResourceID = CommandId.iiconProductIcon)]
    // Declare that resources for the package are to be found in the managed assembly resources, and not in a satellite dll
    [MsVsShell.PackageRegistration(UseManagedResourcesOnly = true)]
    // Register the resource ID of the CTMENU section (generated from compiling the VSCT file), so the IDE will know how to merge this package's menus with the rest of the IDE when "devenv /setup" is run
    // The menu resource ID needs to match the ResourceName number defined in the csproj project file in the VSCTCompile section
    // Everytime the version number changes VS will automatically update the menus on startup; if the version doesn't change, you will need to run manually "devenv /setup /rootsuffix:Exp" to see VSCT changes reflected in IDE
    [MsVsShell.ProvideMenuResource(1000, 1)]
    
    // Register the VisualHg options page visible as Tools/Options/SourceControl/VisualHg when the provider is active
    [MsVsShell.ProvideOptionPage(typeof(SccProviderOptions), "Source Control", "VisualHg", 106, 107, false)]
    [ProvideOptionsPageVisibility("Source Control", "VisualHg", Guids.ProviderGuid)]

    // Register the source control provider's service (implementing IVsScciProvider interface)
    [MsVsShell.ProvideService(typeof(SccProviderService), ServiceName = "VisualHg")]
    // Register the source control provider to be visible in Tools/Options/SourceControl/Plugin dropdown selector
    [ProvideSourceControlProvider("VisualHg", "#100")]
    // Pre-load the package when the command UI context is asserted (the provider will be automatically loaded after restarting the shell if it was active last time the shell was shutdown)
    [MsVsShell.ProvideAutoLoad(Guids.ProviderGuid)]
    // Register the key used for persisting solution properties, so the IDE will know to load the source control package when opening a controlled solution containing properties written by this package
    [ProvideSolutionPersistence(_strSolutionPersistanceKey)]
    [MsVsShell.ProvideLoadKey(PLK.MinEdition, PLK.PackageVersion, PLK.PackageName, PLK.CompanyName, 104)]
    // Declare the package guid
    [Guid(PLK.PackageGuid)]
    public sealed partial class SccProvider : MsVsShell.Package, IOleCommandTarget
    {
        static SccProvider _SccProvider = null;
        // The service provider implemented by the package
        private SccProviderService sccService = null;
        // The name of this provider (to be written in solution and project files)
        // As a best practice, to be sure the provider has an unique name, a guid like the provider guid can be used as a part of the name
        private const string _strProviderName = "VisualHg:"+PLK.PackageGuid;
        // The name of the solution section used to persist provider options (should be unique)
        private const string _strSolutionPersistanceKey = "VisualHgProperties";
        // The name of the section in the solution user options file used to persist user-specific options (should be unique, shorter than 31 characters and without dots)
        private const string _strSolutionUserOptionsKey = "VisualHgSolution";
        // The names of the properties stored by the provider in the solution file
        private const string _strSolutionControlledProperty = "SolutionIsControlled";
        private const string _strSolutionBindingsProperty = "SolutionBindings";

        private IdleNotifier _OnIdleEvent = new IdleNotifier();

        public string _LastSeenProjectDir = string.Empty;

        public SccProvider()
        {
          _SccProvider = this;
          Trace.WriteLine(String.Format(CultureInfo.CurrentUICulture, "Entering constructor for: {0}", this.ToString()));
        }

        void PromptSolutionNotControlled()
        {
            string solutionName = GetSolutionFileName();
            MessageBox.Show("Solution is not under Mercurial version contol\n\n" + solutionName, "VisualHg", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /////////////////////////////////////////////////////////////////////////////
        // SccProvider Package Implementation
        #region Package Members

        public static Object GetServiceEx(Type serviceType)
        {
          if (_SccProvider!=null)
            return _SccProvider.GetService(serviceType);
          return null;  
        }
        
        public new Object GetService(Type serviceType)
        {
            return base.GetService(serviceType);
        }

        public static SccProvider Provider
        {
          get { return _SccProvider; }
        }

        protected override void Initialize()
        {
            Trace.WriteLine(String.Format(CultureInfo.CurrentUICulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Proffer the source control service implemented by the provider
            sccService = new SccProviderService(this);
            ((IServiceContainer)this).AddService(typeof(SccProviderService), sccService, true);
            ((IServiceContainer)this).AddService(typeof(System.IServiceProvider), this, true);
            
            // Add our command handlers for menu (commands must exist in the .vsct file)
            InitializeMenuCommands();
            
            // Register the provider with the source control manager
            // If the package is to become active, this will also callback on OnActiveStateChange and the menu commands will be enabled
            IVsRegisterScciProvider rscp = (IVsRegisterScciProvider)GetService(typeof(IVsRegisterScciProvider));
            rscp.RegisterSourceControlProvider(Guids.guidSccProvider);

            _OnIdleEvent.RegisterForIdleTimeCallbacks(GetGlobalService(typeof(SOleComponentManager)) as IOleComponentManager);
            _OnIdleEvent.Idle += sccService.UpdateDirtyNodesGlyphs;
            
            //ShowToolWindow(VisualHgToolWindow.PendingChanges);
        }

        protected override void Dispose(bool disposing)
        {
            Trace.WriteLine(String.Format(CultureInfo.CurrentUICulture, "Entering Dispose() of: {0}", this.ToString()));

            _OnIdleEvent.Idle -= sccService.UpdateDirtyNodesGlyphs; 
            _OnIdleEvent.UnRegisterForIdleTimeCallbacks();

            _SccProvider = null; 

            sccService.Dispose();
            base.Dispose(disposing);
        }

        // Returns the name of the source control provider
        public string ProviderName
        {
            get { return _strProviderName; }
        }

#endregion

    }
}