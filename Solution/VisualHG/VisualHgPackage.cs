using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using HgLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using OleInteropConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using Process = System.Diagnostics.Process;

namespace VisualHg
{
    [InstalledProductRegistration("#100", "#101", "2.0.0.8", IconResourceID = 400)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideMenuResource(1000, 1)]
    [ProvideAutoLoad(Guids.Provider)]
    [ProvideService(typeof(VisualHgService))]
    [ProvideToolWindow(typeof(PendingChangesToolWindow))]
    [ProvideToolWindowVisibility(typeof(PendingChangesToolWindow), Guids.Provider)]
    [ProvideSolutionPersistence("VisualHg", Guids.Package)]
    [ProvideSourceControlProvider("VisualHg", Guids.Provider, Guids.Service, Guids.Package)]
    [ProvideOptionPage(typeof(VisualHgOptionsPage), "Source Control", "VisualHg", 102, 100, false)]
    [ProvideOptionsPageVisibility("Source Control", "VisualHg", Guids.Provider)]
    [Guid(Guids.Package)]
    public sealed partial class VisualHgPackage : Package, IOleCommandTarget, IDisposable
    {
        private const int OLECMDERR_E_NOTSUPPORTED = (int)OleInteropConstants.OLECMDERR_E_NOTSUPPORTED;

        private VisualHgService visualHgService;
        private PendingChangesToolWindow _pendingChangesToolWindow;


        protected override void Initialize()
        {
            base.Initialize();

            InitializeMenuCommands();

            ((IServiceContainer)this).AddService(typeof(System.IServiceProvider), this, true);

            visualHgService = new VisualHgService();
            ((IServiceContainer)this).AddService(typeof(VisualHgService), visualHgService, true);

            RegisterSourceControlProvider();
        }


        public void Dispose()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                visualHgService.Dispose();
            }

            base.Dispose(disposing);
        }


        public static void RegisterSourceControlProvider()
        {
            var rscp = Package.GetGlobalService(typeof(IVsRegisterScciProvider)) as IVsRegisterScciProvider;
            rscp.RegisterSourceControlProvider(Guids.ProviderGuid);
        }


        public void UpdatePendingChangesToolWindow()
        {
            if (_pendingChangesToolWindow == null)
            {
                _pendingChangesToolWindow = FindToolWindow(typeof(PendingChangesToolWindow), 0, true) as PendingChangesToolWindow;
            }

            _pendingChangesToolWindow.Synchronize(visualHgService.PendingFiles);
        }


        private void NotifySolutionIsNotUnderVersionControl()
        {
            MessageBox.Show(Resources.NotUnderVersionControl + "\n\n" + VisualHgSolution.SolutionFileName, Resources.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void NotifyTortoiseHgNotFound()
        {
            MessageBox.Show(Resources.TortoiseHgNotFound, Resources.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        private void InitializeMenuCommands()
        {
            var menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (menuCommandService != null)
            {
                AddMenuCommands(menuCommandService);
            }
        }

        private void AddMenuCommands(OleMenuCommandService menuCommandService)
        {
            var commandId = new CommandID(Guids.CommandSetGuid, CommandId.PendingChanges);
            var command = new MenuCommand(ShowPendingChangesToolWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.Commit);
            command = new MenuCommand(ShowCommitWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.Workbench);
            command = new MenuCommand(ShowWorkbenchWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.Status);
            command = new MenuCommand(ShowStatusWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.Synchronize);
            command = new MenuCommand(ShowSynchronizeWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.Update);
            command = new MenuCommand(ShowUpdateWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.CreateRepository);
            command = new MenuCommand(ShowCreateRepositoryWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.Settings);
            command = new MenuCommand(ShowSettingsWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.Shelve);
            command = new MenuCommand(ShowShelveWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.Add);
            command = new MenuCommand(ShowAddSelectedWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.CommitSelected);
            command = new MenuCommand(ShowCommitSelectedWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.Diff);
            command = new MenuCommand(ShowDiffWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.Revert);
            command = new MenuCommand(ShowRevertWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.CommandSetGuid, CommandId.History);
            command = new MenuCommand(ShowHistoryWindow, commandId);
            menuCommandService.AddCommand(command);
        }


        public int QueryStatus(ref Guid commandSetGuid, uint commandCount, OLECMD[] commands, IntPtr text)
        {
            if (commandCount != 1)
            {
                return VSConstants.E_INVALIDARG;
            }

            if (commandSetGuid != Guids.CommandSetGuid)
            {
                return OLECMDERR_E_NOTSUPPORTED;
            }

            var visible = visualHgService.Active ? IsCommandVisible(commands[0].cmdID) : false;
            var enabled = visualHgService.Active ? IsCommandEnabled(commands[0].cmdID) : false;

            commands[0].cmdf = (uint)ToOleCmdf(visible, enabled);
            
            return VSConstants.S_OK;
        }

        private OLECMDF ToOleCmdf(bool visible, bool enabled)
        {
            var cmdf = OLECMDF.OLECMDF_SUPPORTED;

            if (enabled)
            {
                cmdf |= OLECMDF.OLECMDF_ENABLED;
            }

            if (!visible)
            {
                cmdf |= OLECMDF.OLECMDF_INVISIBLE;
            }

            return cmdf;
        }

        private bool IsCommandEnabled(uint commandId)
        {
            switch (commandId)
            {
                case CommandId.Settings:
                case CommandId.Workbench:
                case CommandId.CreateRepository:
                case CommandId.PendingChanges:
                    return true;

                default:
                    return !String.IsNullOrEmpty(VisualHgSolution.SolutionFileName);
            }
        }

        private bool IsCommandVisible(uint commandId)
        {
            switch (commandId)
            {
                case CommandId.Add:
                    return IsAddMenuItemVisible();

                case CommandId.CommitSelected:
                    return IsCommitSelectedMenuItemVisible();

                case CommandId.Diff:
                    return IsDiffMenuItemVisible();

                case CommandId.Revert:
                    return IsRevertMenuItemVisible();

                case CommandId.History:
                    return IsHistoryMenuItemVisible();

                default:
                    return true;
            }
        }

        private bool IsAddMenuItemVisible()
        {
            return VisualHgSolution.SearchAnySelectedFileStatusMatches(HgFileStatus.NotAdded);
        }

        private bool IsCommitSelectedMenuItemVisible()
        {
            return VisualHgSolution.SearchAnySelectedFileStatusMatches(HgFileStatus.Pending);
        }

        private bool IsDiffMenuItemVisible()
        {
            return VisualHgSolution.SelectedFileStatusMatches(HgFileStatus.Comparable);
        }

        private bool IsRevertMenuItemVisible()
        {
            return VisualHgSolution.SearchAnySelectedFileStatusMatches(HgFileStatus.Pending);
        }

        private bool IsHistoryMenuItemVisible()
        {
            return VisualHgSolution.SelectedFileStatusMatches(HgFileStatus.Tracked);
        }


        private void ShowPendingChangesToolWindow(object sender, EventArgs e)
        {
            if (_pendingChangesToolWindow == null)
            {
                UpdatePendingChangesToolWindow();
            }

            var windowFrame = _pendingChangesToolWindow.Frame as IVsWindowFrame;

            if (windowFrame != null)
            {
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
        }


        private void ShowCommitWindow(object sender, EventArgs e)
        {
            CheckAndShow(TortoiseHg.ShowCommitWindow);
        }

        private void ShowStatusWindow(object sender, EventArgs e)
        {
            CheckAndShow(TortoiseHg.ShowStatusWindow);
        }

        private void ShowSynchronizeWindow(object sender, EventArgs e)
        {
            CheckAndShow(TortoiseHg.ShowSynchronizeWindow);
        }

        private void ShowShelveWindow(object sender, EventArgs e)
        {
            CheckAndShow(TortoiseHg.ShowShelveWindow);
        }


        private void CheckAndShow(Func<string, System.Diagnostics.Process> show)
        {
            var root = VisualHgSolution.CurrentRootDirectory;

            if (CanRunTortoiseHg(root))
            {
                show(root);
            }
        }

        private bool CanRunTortoiseHg(string root)
        {
            if (TortoiseHg.Version == null)
            {
                NotifyTortoiseHgNotFound();
                return false;
            }

            if (String.IsNullOrEmpty(root))
            {
                NotifySolutionIsNotUnderVersionControl();
                return false;
            }

            SaveAllProjectFiles();
            return true;
        }

        
        private void ShowUpdateWindow(object sender, EventArgs e)
        {
            var root = VisualHgSolution.CurrentRootDirectory;

            if (!CanRunTortoiseHg(root))
            {
                return;
            }

            if (IsReloadSolutionNeeded())
            {
                ShowUpdateDialogAndReloadSolution(root);
            }
            else
            {
                TortoiseHg.ShowUpdateWindow(root);
            }
        }

        private bool IsReloadSolutionNeeded()
        {
            if (VsVersion > 10)
            {
                return false;
            }

            if (VisualHgSolution.LoadedProjects.Take(2).Count() < 2)
            {
                return false;
            }

            var result = MessageBox.Show(Resources.SolutionReloadQuery, Resources.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        private void ShowUpdateDialogAndReloadSolution(string root)
        {
            var dte = GetService(typeof(SDTE)) as DTE;

            var solutionFileName = VisualHgSolution.SolutionFileName;

            dte.Solution.Close();

            WaitForExit(TortoiseHg.ShowUpdateWindow(root));

            dte.Solution.Open(solutionFileName);
        }

        private static void WaitForExit(Process process)
        {
            if (process == null)
            {
                return;
            }

            try
            {
                process.WaitForExit();
            }
            catch (InvalidOperationException) { }

            WaitForExit(GetChildProcess(process));
        }

        private static Process GetChildProcess(Process process)
        {
            return ProcessInfo.GetChildProcesses(process).FirstOrDefault();
        }
        
        
        private void ShowWorkbenchWindow(object sender, EventArgs e)
        {
            if (TortoiseHg.Version == null)
            {
                NotifyTortoiseHgNotFound();
                return;
            }

            TortoiseHg.ShowWorkbenchWindow(VisualHgSolution.CurrentRootDirectory ?? "");
        }

        private void ShowSettingsWindow(object sender, EventArgs e)
        {
            if (TortoiseHg.Version == null)
            {
                NotifyTortoiseHgNotFound();
                return;
            }

            var root = VisualHgSolution.CurrentRootDirectory;

            if (String.IsNullOrEmpty(root))
            {
                TortoiseHg.ShowUserSettingsWindow("");
            }
            else
            {
                TortoiseHg.ShowRepositorySettingsWindow(root);
            }
        }

        private void ShowCreateRepositoryWindow(object sender, EventArgs e)
        {
            var directory = Path.GetDirectoryName(VisualHgSolution.SolutionFileName) ??
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            TortoiseHg.ShowCreateRepositoryWindow(directory);
        }


        private void ShowAddSelectedWindow(object sender, EventArgs e)
        {
            if (TortoiseHg.Version == null)
            {
                NotifyTortoiseHgNotFound();
                return;
            }

            var filesToAdd = GetSelectedFiles().Where(VisualHgFileStatus.IsNotAdded).ToArray();

            if (filesToAdd.Length > 0)
            {
                TortoiseHg.ShowAddWindow(filesToAdd);
            }
        }

        private void ShowCommitSelectedWindow(object sender, EventArgs e)
        {
            if (TortoiseHg.Version == null)
            {
                NotifyTortoiseHgNotFound();
                return;
            }

            SaveAllProjectFiles();
            VisualHgDialogs.ShowCommitWindow(GetSelectedFiles());
        }

        private void ShowDiffWindow(object sender, EventArgs e)
        {
            VisualHgDialogs.ShowDiffWindow(VisualHgSolution.SelectedFile);
        }

        private void ShowRevertWindow(object sender, EventArgs e)
        {
            if (TortoiseHg.Version == null)
            {
                NotifyTortoiseHgNotFound();
                return;
            }

            SaveAllProjectFiles();
            VisualHgDialogs.ShowRevertWindow(GetSelectedFiles());
        }

        private void ShowHistoryWindow(object sender, EventArgs e)
        {
            if (TortoiseHg.Version == null)
            {
                NotifyTortoiseHgNotFound();
                return;
            }

            VisualHgDialogs.ShowHistoryWindow(VisualHgSolution.SelectedFile);
        }


        private string[] GetSelectedFiles()
        {
            return VisualHgSolution.GetSelectedFiles(VisualHgOptions.Global.ProjectStatusIncludesChildren);
        }


        private void SaveAllProjectFiles()
        {
            foreach (var project in VisualHgSolution.LoadedProjects)
            {
                SaveProject(project);
            }
        }

        private void SaveProject(IVsHierarchy project)
        {
            var solution = Package.GetGlobalService(typeof(IVsSolution)) as IVsSolution;
            var options = (uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty;

            solution.SaveSolutionElement(options, project, 0);
        }


        private static int _vsVersion;

        public static int VsVersion
        {
            get
            {
                if (_vsVersion == 0)
                {
                    var version = GetVersion();
                    var majorVersion = version.Substring(0, version.IndexOf('.'));

                    if (!Int32.TryParse(majorVersion, out _vsVersion))
                    {
                         _vsVersion = 10;
                    }
                }

                return _vsVersion;
            }
        }

        private static string GetVersion()
        {
            var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            
            return dte.Version;
        }
    }
}