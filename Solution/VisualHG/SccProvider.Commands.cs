using System;
using System.Collections.Generic;
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

            commandId = new CommandID(Guids.guidSccProviderCmdSet, CommandId.icmdHgAnnotate);
            command = new MenuCommand(ShowAnnotateWindow, commandId);
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

                case CommandId.icmdHgAnnotate:
                    cmdf = VisibleToOleCmdf(IsHgAnnotateMenuItemVisible());
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
            return SelectedFileContextStatusMatches
               (HgFileStatus.Modified |
                HgFileStatus.Added |
                HgFileStatus.Copied |
                HgFileStatus.Renamed |
                HgFileStatus.Removed,
                true);
        }

        private bool IsHgDiffMenuItemVisible()
        {
            return SelectedFileStatusMatches
               (HgFileStatus.Modified |
                HgFileStatus.Removed |
                HgFileStatus.Renamed |
                HgFileStatus.Copied |
                HgFileStatus.Missing);
        }

        private bool IsHgRevertMenuItemVisible()
        {
            return SelectedFileContextStatusMatches
               (HgFileStatus.Added |
                HgFileStatus.Copied |
                HgFileStatus.Modified |
                HgFileStatus.Renamed |
                HgFileStatus.Removed);
        }

        private bool IsHgHistoryMenuItemVisible()
        {
            return SelectedFileStatusMatches
                (HgFileStatus.Clean |
                 HgFileStatus.Modified |
                 HgFileStatus.Removed |
                 HgFileStatus.Missing);
        }

        private bool IsHgAnnotateMenuItemVisible()
        {
            return IsHgHistoryMenuItemVisible();
        }


        private void ShowPendingChangesToolWindow(object sender, EventArgs e)
        {
            var window = FindToolWindow(typeof(HgPendingChangesToolWindow), 0, true);
            var windowFrame = window != null ? window.Frame as IVsWindowFrame : null;

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
            StoreSolution();

            var root = GetRootDirectory();

            if (!String.IsNullOrEmpty(root))
            {
                showWindow(root);
            }
            else
            {
                PromptSolutionNotControlled();
            }
        }


        private void ShowAddSelectedWindow(object sender, EventArgs e)
        {
            StoreSolution();

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
            ShowDiffWindow(GetSelectedFile());
        }

        private void ShowRevertWindow(object sender, EventArgs e)
        {
            ShowRevertWindow(GetSelectedFiles(false));
        }

        private void ShowHistoryWindow(object sender, EventArgs e)
        {
            ShowHistoryWindow(GetSelectedFile());
        }

        private void ShowAnnotateWindow(object sender, EventArgs e)
        {
            ShowAnnotateWindow(GetSelectedFile());
        }


        public void ShowCommitWindow(IEnumerable<string> files)
        {
            StoreSolution();

            var filesToCommit = files.Where(FileIsDirty).ToArray();

            if (filesToCommit.Length > 0)
            {
                ShowCommitWindowPrivate(filesToCommit);
            }
        }

        public void ShowDiffWindow(string fileName)
        {
            StoreSolution();

            if (!String.IsNullOrEmpty(fileName))
            {
                return;
            }

            var status = sccService.GetFileStatus(fileName);

            if (status == HgFileStatus.Uncontrolled &&
                status == HgFileStatus.Added &&
                status == HgFileStatus.Ignored)
            {
                return;
            }
         
            var parent = fileName;

            if (status == HgFileStatus.Renamed || status == HgFileStatus.Copied)
            {
                parent = Hg.GetRenamedFileOriginalName(fileName);
            }

            if (!String.IsNullOrEmpty(parent))
            {
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
        }

        public void ShowRevertWindow(IEnumerable<string> files)
        {
            StoreSolution();

            var filesToRevert = files.Where(FileIsDirty).ToArray();

            if (filesToRevert.Length > 0)
            {
                ShowRevertWindowPrivate(filesToRevert.ToArray());
            }
        }

        public void ShowHistoryWindow(string fileName)
        {
            StoreSolution();

            if (FileStatusMatches(fileName,
                HgFileStatus.Clean |
                HgFileStatus.Modified |
                HgFileStatus.Removed |
                HgFileStatus.Renamed |
                HgFileStatus.Copied |
                HgFileStatus.Missing))
            {
                ShowHistoryWindowPrivate(fileName);
            }
        }

        public void ShowAnnotateWindow(string fileName)
        {
            StoreSolution();

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
                ShowAnnotateWindowPrivate(fileName);
            }
        }
        

        private bool SelectedFileContextStatusMatches(HgFileStatus status, bool includeChildren = false)
        {
            if (Configuration.Global.EnableContextSearch)
            {
                return FindSelectedFirstMask(status, includeChildren);
            }

            return true;
        }

        private bool SelectedFileStatusMatches(HgFileStatus status)
        {
            return FileStatusMatches(GetSelectedFile(), status);
        }


        private bool FileIsNotAdded(string fileName)
        {
            return FileStatusMatches(fileName, HgFileStatus.Uncontrolled | HgFileStatus.Ignored);
        }

        private bool FileIsDirty(string fileName)
        {
            return FileStatusMatches(fileName,
                HgFileStatus.Modified |
                HgFileStatus.Added |
                HgFileStatus.Removed |
                HgFileStatus.Renamed |
                HgFileStatus.Copied |
                HgFileStatus.Missing);
        }
        

        private bool FileStatusMatches(string fileName, HgFileStatus status)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                return false;
            }

            var fileStatus = sccService.GetFileStatus(fileName);

            return (int)(status & fileStatus) > 0;
        }
    }
}