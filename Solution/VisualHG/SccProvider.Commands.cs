using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows.Forms;
using HgLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using OleInterop = Microsoft.VisualStudio.OLE.Interop;

namespace VisualHg
{
    partial class SccProvider
    {
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
            var commandId = new CommandID(Guids.guidSccProviderCmdSet, CommandId.icmdViewToolWindow);
            var command = new MenuCommand(ShowPendingChangesToolWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.guidSccProviderCmdSet, CommandId.icmdHgCommit);
            command = new MenuCommand(ShowCommitWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.guidSccProviderCmdSet, CommandId.icmdHgWorkbench);
            command = new MenuCommand(ShowWorkbenchWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.guidSccProviderCmdSet, CommandId.icmdHgStatus);
            command = new MenuCommand(ShowStatusWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.guidSccProviderCmdSet, CommandId.icmdHgSynchronize);
            command = new MenuCommand(ShowSynchronizeWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.guidSccProviderCmdSet, CommandId.icmdHgUpdate);
            command = new MenuCommand(ShowUpdateWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.guidSccProviderCmdSet, CommandId.icmdHgAddSelected);
            command = new MenuCommand(ShowAddSelectedWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.guidSccProviderCmdSet, CommandId.icmdHgCommitSelected);
            command = new MenuCommand(ShowCommitSelectedWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.guidSccProviderCmdSet, CommandId.icmdHgDiff);
            command = new MenuCommand(ShowDiffWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.guidSccProviderCmdSet, CommandId.icmdHgRevert);
            command = new MenuCommand(ShowRevertWindow, commandId);
            menuCommandService.AddCommand(command);

            commandId = new CommandID(Guids.guidSccProviderCmdSet, CommandId.icmdHgHistory);
            command = new MenuCommand(ShowHistoryWindow, commandId);
            menuCommandService.AddCommand(command);
        }


        public int QueryStatus(ref Guid guidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            OLECMDF cmdf;

            if (prgCmds == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            if (guidCmdGroup != Guids.guidSccProviderCmdSet)
            {
                return (int)OleInterop.Constants.OLECMDERR_E_NOTSUPPORTED;
            }

            if (!sccService.Active)
            {
                prgCmds[0].cmdf = (uint)VisibleToOleCmdf(false);

                return VSConstants.S_OK;
            }

            switch (prgCmds[0].cmdID)
            {
                case CommandId.icmdViewToolWindow:
                case CommandId.icmdHgCommit:
                case CommandId.icmdHgWorkbench:
                case CommandId.icmdHgStatus:
                case CommandId.icmdHgSynchronize:
                case CommandId.icmdHgUpdate:
                    cmdf = VisibleToOleCmdf(true);
                    break;

                case CommandId.icmdHgAddSelected:
                    cmdf = VisibleToOleCmdf(IsHgAddSelectedMenuItemVisible());
                    break;

                case CommandId.icmdHgCommitSelected:
                    cmdf = VisibleToOleCmdf(IsHgCommitSelectedMenuItemVisible());
                    break;

                case CommandId.icmdHgDiff:
                    cmdf = VisibleToOleCmdf(IsHgDiffMenuItemVisible());
                    break;

                case CommandId.icmdHgRevert:
                    cmdf = VisibleToOleCmdf(IsHgRevertMenuItemVisible());
                    break;

                case CommandId.icmdHgHistory:
                    cmdf = VisibleToOleCmdf(IsHgHistoryMenuItemVisible());
                    break;

                default:
                    return (int)(OleInterop.Constants.OLECMDERR_E_NOTSUPPORTED);
            }

            prgCmds[0].cmdf = (uint)cmdf;

            return VSConstants.S_OK;
        }

        private OLECMDF VisibleToOleCmdf(bool visible)
        {
            return OLECMDF.OLECMDF_SUPPORTED | (visible ? OLECMDF.OLECMDF_ENABLED : OLECMDF.OLECMDF_INVISIBLE);
        }

        private bool IsHgAddSelectedMenuItemVisible()
        {
            return SelectedFileContextStatusMatches(HgFileStatus.Uncontrolled | HgFileStatus.Ignored);
        }

        private bool IsHgCommitSelectedMenuItemVisible()
        {
            return SelectedFileContextStatusMatches(HgFileStatus.Different, true);
        }

        private bool IsHgDiffMenuItemVisible()
        {
            return SelectedFileStatusMatches(HgFileStatus.Comparable);
        }

        private bool IsHgRevertMenuItemVisible()
        {
            return SelectedFileContextStatusMatches(HgFileStatus.Different);
        }

        private bool IsHgHistoryMenuItemVisible()
        {
            return SelectedFileStatusMatches(HgFileStatus.Controlled);
        }


        private void ShowPendingChangesToolWindow(object sender, EventArgs e)
        {
            var windowFrame = PendingChangesToolWindow.Frame as IVsWindowFrame;

            if (windowFrame != null)
            {
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
        }


        private void ShowCommitWindow(object sender, EventArgs e)
        {
            GetRootAnd(ShowCommitWindow);
        }

        private void ShowWorkbenchWindow(object sender, EventArgs e)
        {
            GetRootAnd(ShowWorkbenchWindow);
        }

        private void ShowStatusWindow(object sender, EventArgs e)
        {
            GetRootAnd(ShowStatusWindow);
        }

        private void ShowSynchronizeWindow(object sender, EventArgs e)
        {
            GetRootAnd(ShowSynchronizeWindow);
        }

        private void ShowUpdateWindow(object sender, EventArgs e)
        {
            GetRootAnd(ShowUpdateWindow);
        }

        private void GetRootAnd(Action<string> showWindow)
        {
            SaveSolutionIfDirty();

            var root = CurrentRootDirectory;

            if (!String.IsNullOrEmpty(root))
            {
                showWindow(root);
            }
            else
            {
                NotifySolutionIsNotUnderVersionControl();
            }
        }


        private void ShowAddSelectedWindow(object sender, EventArgs e)
        {
            SaveSolutionIfDirty();

            var filesToAdd = GetSelectedFiles(false).Where(FileIsNotAdded).ToArray();

            if (filesToAdd.Length > 0)
            {
                ShowAddSelectedWindow(filesToAdd);
            }
        }

        private void ShowCommitSelectedWindow(object sender, EventArgs e)
        {
            ShowCommitWindow(GetSelectedFiles(true));
        }

        private void ShowDiffWindow(object sender, EventArgs e)
        {
            ShowDiffWindow(SelectedFile);
        }

        private void ShowRevertWindow(object sender, EventArgs e)
        {
            ShowRevertWindow(GetSelectedFiles(false));
        }

        private void ShowHistoryWindow(object sender, EventArgs e)
        {
            ShowHistoryWindow(SelectedFile);
        }


        public void ShowCommitWindow(string[] files)
        {
            SaveSolutionIfDirty();

            var filesToCommit = files.Where(FileIsDirty).ToArray();

            if (filesToCommit.Length > 0)
            {
                ShowCommitWindowPrivate(filesToCommit);
            }
        }

        public void ShowDiffWindow(string fileName)
        {
            SaveSolutionIfDirty();

            if (String.IsNullOrEmpty(fileName))
            {
                return;
            }

            if (FileStatusMatches(fileName, HgFileStatus.Uncontrolled | HgFileStatus.Added | HgFileStatus.Ignored))
            {
                return;
            }

            var parent = fileName;

            if (FileStatusMatches(fileName, HgFileStatus.Renamed | HgFileStatus.Copied))
            {
                parent = Hg.GetRenamedFileOriginalName(fileName);
            }

            if (String.IsNullOrEmpty(parent))
            {
                return;
            }

            try
            {
                ShowDiffWindow(parent, fileName, Configuration.Global.ExternalDiffToolCommandMask);
            }
            catch
            {
                if (!String.IsNullOrEmpty(Configuration.Global.ExternalDiffToolCommandMask))
                {
                    MessageBox.Show("The DiffTool raised an error\nPlease check your command mask:\n\n" + Configuration.Global.ExternalDiffToolCommandMask, "VisualHg", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        public void ShowRevertWindow(string[] files)
        {
            SaveSolutionIfDirty();

            var filesToRevert = files.Where(FileIsDirty).ToArray();

            if (filesToRevert.Length > 0)
            {
                ShowRevertWindowPrivate(filesToRevert.ToArray());
            }
        }

        public void ShowHistoryWindow(string fileName)
        {
            SaveSolutionIfDirty();

            var status = HgFileStatus.Ignored;

            if (!String.IsNullOrEmpty(fileName))
            {
                status = sccService.GetFileStatus(fileName);
            }

            if (status == HgFileStatus.Renamed)
            {
                fileName = Hg.GetRenamedFileOriginalName(fileName);
            }

            if (!String.IsNullOrEmpty(fileName) &&
                status != HgFileStatus.Uncontrolled &&
                status != HgFileStatus.Ignored)
            {
                ShowHistoryWindowPrivate(fileName);
            }
        }
    }
}