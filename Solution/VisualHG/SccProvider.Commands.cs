using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel.Design;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;
using MsVsShell = Microsoft.VisualStudio.Shell;
using System.Windows.Forms;

namespace VisualHg
{
    /// <summary>
    /// SccProvider VSCT defined menu command handler
    /// </summary>
    partial class SccProvider
    {
        /// <summary>
        /// Add our command handlers for menu (commands must exist in the .vsct file)
        /// </summary>
        void InitVSCTMenuCommandHandler()
        {
            MsVsShell.OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as MsVsShell.OleMenuCommandService;
            if (mcs != null)
            {
                // ToolWindow Command
                CommandID cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdViewToolWindow);
                MenuCommand menuCmd = new MenuCommand(new EventHandler(Exec_icmdViewToolWindow), cmd);
                mcs.AddCommand(menuCmd);

                // ToolWindow's ToolBar Command
                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdToolWindowToolbarCommand);
                menuCmd = new MenuCommand(new EventHandler(Exec_icmdToolWindowToolbarCommand), cmd);
                mcs.AddCommand(menuCmd);

                // Source control menu commmads
                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdHgStatus);
                menuCmd = new MenuCommand(new EventHandler(Exec_icmdHgStatus), cmd);
                mcs.AddCommand(menuCmd);
                
                // Source control menu commmads
                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdHgDiff);
                menuCmd = new MenuCommand(new EventHandler(Exec_icmdHgDiff), cmd);
                mcs.AddCommand(menuCmd);

                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdHgCommitRoot);
                menuCmd = new MenuCommand(new EventHandler(Exec_icmdHgCommitRoot), cmd);
                mcs.AddCommand(menuCmd);

                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdHgCommitSelected);
                menuCmd = new MenuCommand(new EventHandler(Exec_icmdHgCommitSelected), cmd);
                mcs.AddCommand(menuCmd);

                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdHgHistoryRoot);
                menuCmd = new MenuCommand(new EventHandler(Exec_icmdHgHistoryRoot), cmd);
                mcs.AddCommand(menuCmd);

                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdHgHistorySelected);
                menuCmd = new MenuCommand(new EventHandler(Exec_icmdHgHistorySelected), cmd);
                mcs.AddCommand(menuCmd);

                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdHgSynchronize);
                menuCmd = new MenuCommand(new EventHandler(Exec_icmdHgSynchronize), cmd);
                mcs.AddCommand(menuCmd);

                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdHgUpdateToRevision);
                menuCmd = new MenuCommand(new EventHandler(Exec_icmdHgUpdateToRevision), cmd);
                mcs.AddCommand(menuCmd);

                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdHgRevert);
                menuCmd = new MenuCommand(new EventHandler(Exec_icmdHgRevert), cmd);
                mcs.AddCommand(menuCmd);

                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdHgAnnotate);
                menuCmd = new MenuCommand(new EventHandler(Exec_icmdHgAnnotate), cmd);
                mcs.AddCommand(menuCmd);

                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdHgAddSelected);
                menuCmd = new MenuCommand(new EventHandler(Exec_icmdHgAddSelected), cmd);
                mcs.AddCommand(menuCmd);

            }
        }

        #region Source Control Command Enabling IOleCommandTarget.QueryStatus

        /// <summary>
        /// The shell call this function to know if a menu item should be visible and
        /// if it should be enabled/disabled.
        /// Note that this function will only be called when an instance of this editor
        /// is open.
        /// </summary>
        /// <param name="guidCmdGroup">Guid describing which set of command the current command(s) belong to</param>
        /// <param name="cCmds">Number of command which status are being asked for</param>
        /// <param name="prgCmds">Information for each command</param>
        /// <param name="pCmdText">Used to dynamically change the command text</param>
        /// <returns>HRESULT</returns>
        public int QueryStatus(ref Guid guidCmdGroup, uint cCmds, OLECMD[] prgCmds, System.IntPtr pCmdText)
        {
            Debug.Assert(cCmds == 1, "Multiple commands");
            Debug.Assert(prgCmds != null, "NULL argument");

            if ((prgCmds == null))
                return VSConstants.E_INVALIDARG;

            // Filter out commands that are not defined by this package
            if (guidCmdGroup != GuidList.guidSccProviderCmdSet)
            {
                return (int)(Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED); ;
            }

            OLECMDF cmdf = OLECMDF.OLECMDF_SUPPORTED;

            // All source control commands needs to be hidden and disabled when the provider is not active
            if (!sccService.Active)
            {
                cmdf = OLECMDF.OLECMDF_INVISIBLE;

                prgCmds[0].cmdf = (uint)cmdf;
                return VSConstants.S_OK;
            }

            // Process our Commands
            switch (prgCmds[0].cmdID)
            {
                case CommandId.icmdHgStatus:
                    cmdf = QueryStatus_icmdHgStatus();
                    break;

                case CommandId.icmdHgCommitRoot:
                    cmdf = QueryStatus_icmdHgCommitRoot();
                    break;

                case CommandId.icmdHgCommitSelected:
                    cmdf = QueryStatus_icmdHgCommitSelected();
                    break;

                case CommandId.icmdHgHistoryRoot:
                    cmdf = QueryStatus_icmdHgHistoryRoot();
                    break;

                case CommandId.icmdHgHistorySelected:
                    cmdf = QueryStatus_icmdHgHistorySelected();
                    break;

                case CommandId.icmdHgSynchronize:
                    cmdf = QueryStatus_icmdHgSynchronize();
                    break;

                case CommandId.icmdHgUpdateToRevision:
                    cmdf = QueryStatus_icmdHgUpdateToRevision();
                    break;

                case CommandId.icmdHgDiff:
                    cmdf = QueryStatus_icmdHgDiff();
                    break;

                case CommandId.icmdHgRevert:
                    cmdf = QueryStatus_icmdHgRevert();
                    break;

                case CommandId.icmdHgAnnotate:
                    cmdf = QueryStatus_icmdHgAnnotate();
                    break;

                case CommandId.icmdHgAddSelected:
                    cmdf = QueryStatus_icmdHgAddSelected();
                    break;

                case CommandId.icmdViewToolWindow:
                case CommandId.icmdToolWindowToolbarCommand:
                    // These commmands are always enabled when the provider is active
                    cmdf = OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;;
                    break;

                default:
                    return (int)(Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED);
            }

            prgCmds[0].cmdf = (uint)cmdf;

            return VSConstants.S_OK;
        }

        OLECMDF QueryStatus_icmdHgCommitRoot()
        {
            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
        }

        OLECMDF QueryStatus_icmdHgAddSelected()
        {
            OLECMDF cmdf = OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE;

            long stateMask  =  (long)HgLib.HgFileStatus.Uncontrolled |
                               (long)HgLib.HgFileStatus.Ignored;

            if (!Configuration.Global.EnableContextSearch || FindSelectedFirstMask(false, stateMask))
            {
                cmdf = OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
            }

            return cmdf;
        }
        OLECMDF QueryStatus_icmdHgCommitSelected()
        {
            OLECMDF cmdf = OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE;

            long stateMask = (long)HgLib.HgFileStatus.Modified |
                             (long)HgLib.HgFileStatus.Added|
                             (long)HgLib.HgFileStatus.Copied|
                             (long)HgLib.HgFileStatus.Renamed|
                             (long)HgLib.HgFileStatus.Removed;

            if (!Configuration.Global.EnableContextSearch || FindSelectedFirstMask(true, stateMask))
            {
                cmdf = OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
            }

            return cmdf;
        }


        OLECMDF QueryStatus_icmdHgHistoryRoot()
        {
            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
        }

        OLECMDF QueryStatus_icmdHgHistorySelected()
        {
            string filename = GetSingleSelectedFileName();
            if (filename != string.Empty)
            {
                HgLib.HgFileStatus status = this.sccService.GetFileStatus(filename);
                if (status != HgLib.HgFileStatus.Uncontrolled &&
                    status != HgLib.HgFileStatus.Ignored &&
                    status != HgLib.HgFileStatus.Added &&
                    status != HgLib.HgFileStatus.Renamed &&
                    status != HgLib.HgFileStatus.Copied)
                    return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
            }

            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE;
        }

        OLECMDF QueryStatus_icmdHgStatus()
        {
            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
        }

        OLECMDF QueryStatus_icmdHgSynchronize()
        {
            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
        }

        OLECMDF QueryStatus_icmdHgUpdateToRevision()
        {
            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
        }

        OLECMDF QueryStatus_icmdHgDiff()
        {
            string filename = GetSingleSelectedFileName();

            if (filename != String.Empty)
            {
                HgLib.HgFileStatus status = this.sccService.GetFileStatus(filename);
                if (status != HgLib.HgFileStatus.Uncontrolled &&
                    status != HgLib.HgFileStatus.Added &&
                    status != HgLib.HgFileStatus.Ignored &&
                    status != HgLib.HgFileStatus.Clean)
                    return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;

            }

            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE;
        }

        OLECMDF QueryStatus_icmdHgRevert()
        {
            OLECMDF cmdf = OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE;

            long stateMask = (long)HgLib.HgFileStatus.Added |
                             (long)HgLib.HgFileStatus.Copied |
                             (long)HgLib.HgFileStatus.Modified|
                             (long)HgLib.HgFileStatus.Renamed |
                             (long)HgLib.HgFileStatus.Removed;

            if (!Configuration.Global.EnableContextSearch || FindSelectedFirstMask(false, stateMask))
            {
                cmdf = OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
            }

            return cmdf;
        }

        OLECMDF QueryStatus_icmdHgAnnotate()
        {
            string filename = GetSingleSelectedFileName();
            if (filename != String.Empty)
            {
                HgLib.HgFileStatus status = this.sccService.GetFileStatus(filename);
                if (status != HgLib.HgFileStatus.Uncontrolled &&
                    status != HgLib.HgFileStatus.Ignored &&
                    status != HgLib.HgFileStatus.Added &&
                    status != HgLib.HgFileStatus.Renamed &&
                    status != HgLib.HgFileStatus.Copied)
                    return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;

            }

            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE;
        }


        #endregion

        #region Source Control Commands Execution

        private void Exec_icmdHgCommitRoot(object sender, EventArgs e)
        {
            StoreSolution();

            string root = GetRootDirectory();
            if (root != string.Empty)
              CommitDialog(root);
            else
              PromptSolutionNotControlled();
        }

        private void Exec_icmdHgCommitSelected(object sender, EventArgs e)
        {
          List<string> array = GetSelectedFileNameArray(true);
          CommitDialog(array);
        }

        public void CommitDialog(List<string> array)
        {
            StoreSolution();

            List<string> commitList = new List<string>();
            foreach (string name in array)
            {
                HgLib.HgFileStatus status = this.sccService.GetFileStatus(name);
                if (status != HgLib.HgFileStatus.Uncontrolled &&
                    status != HgLib.HgFileStatus.Clean &&
                    status != HgLib.HgFileStatus.Ignored)
                {
                    commitList.Add(name);
                }
            }

            if (commitList.Count > 0)
            {
                CommitDialog(commitList.ToArray());
            }
        }

        private void Exec_icmdHgAddSelected(object sender, EventArgs e)
        {
            List<string> array = GetSelectedFileNameArray(false);
            HgAddSelected(array);
        }

        public void HgAddSelected(List<string> array)
        {
            StoreSolution();

            List<string> addList = new List<string>();
            foreach (string name in array)
            {
                HgLib.HgFileStatus status = this.sccService.GetFileStatus(name);
                if (status == HgLib.HgFileStatus.Uncontrolled ||
                    status == HgLib.HgFileStatus.Ignored)
                {
                    addList.Add(name);
                }
            }

            if (addList.Count > 0)
            {
                AddFilesDialog(addList.ToArray());
            }
        }

        private void Exec_icmdHgHistoryRoot(object sender, EventArgs e)
        {
            StoreSolution();

            string root = GetRootDirectory();

            if (root != null && root != String.Empty)
                RepoBrowserDialog(root);
            else
                PromptSolutionNotControlled();
        }

        private void Exec_icmdHgHistorySelected(object sender, EventArgs e)
        {
          string fileName = GetSingleSelectedFileName();
          ShowHgHistoryDlg(fileName);
        }

        public void ShowHgHistoryDlg(string fileName)
        {
            StoreSolution();
            
            if (fileName != string.Empty)
            {
                HgLib.HgFileStatus status = this.sccService.GetFileStatus(fileName);
                if (status != HgLib.HgFileStatus.Uncontrolled &&
                    status != HgLib.HgFileStatus.Added &&
                    status != HgLib.HgFileStatus.Ignored)
                {
                    LogDialog(fileName); 
                }
            }
        }

        private void Exec_icmdHgStatus(object sender, EventArgs e)
        {
            StoreSolution();

            string root = GetRootDirectory();
            if (root != string.Empty)
                StatusDialog(root);
            else
                PromptSolutionNotControlled();

        }

        private void Exec_icmdHgDiff(object sender, EventArgs e)
        {
          string fileName = GetSingleSelectedFileName();
          ShowHgDiffDlg(fileName);
        }

        public void ShowHgDiffDlg(string fileName)
        {
            StoreSolution();

            if (fileName != String.Empty)
            {
                string versionedFile = fileName;

                HgLib.HgFileStatus status = this.sccService.GetFileStatus(fileName);
                if (status != HgLib.HgFileStatus.Uncontrolled &&
                    status != HgLib.HgFileStatus.Added &&
                    status != HgLib.HgFileStatus.Ignored)
                {
                    if (status == HgLib.HgFileStatus.Renamed ||
                        status == HgLib.HgFileStatus.Copied)
                    {
                        versionedFile = HgLib.Hg.GetRenamedFileOriginalName(fileName); 
                    }

                    if (versionedFile != null)
                    { 
                        try
                        { 
                            DiffDialog(versionedFile, fileName, Configuration.Global.ExternalDiffToolCommandMask);
                        }
                        catch
                        {
                            if (Configuration.Global.ExternalDiffToolCommandMask != string.Empty)
                                MessageBox.Show("The DiffTool raised an error\nPlease check your command mask:\n\n" + Configuration.Global.ExternalDiffToolCommandMask,
                                                "VisualHg",
                                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }                        
                    }
                }
            }
        }

        private void Exec_icmdHgSynchronize(object sender, EventArgs e)
        {
            StoreSolution();

            string root = GetRootDirectory();
            if (root != string.Empty)
                SyncDialog(root);
            else
                PromptSolutionNotControlled();
        }

        private void Exec_icmdHgUpdateToRevision(object sender, EventArgs e)
        {
            StoreSolution();

            string root = GetRootDirectory();
            if (root != string.Empty)
                HgLib.TortoiseHg.ShowUpdateWindow(root);
            else
                PromptSolutionNotControlled();
        }

        private void Exec_icmdHgRevert(object sender, EventArgs e)
        {
            List<string> array = GetSelectedFileNameArray(false);
            HgRevertFileDlg(array.ToArray());
        }

        public void HgRevertFileDlg(string[] array)
        {
            StoreSolution();

            List<string> addList = new List<string>();
            foreach (string name in array)
            {
                HgLib.HgFileStatus status = this.sccService.GetFileStatus(name);
                if (status == HgLib.HgFileStatus.Modified ||
                    status == HgLib.HgFileStatus.Added ||
                    status == HgLib.HgFileStatus.Copied ||
                    status == HgLib.HgFileStatus.Removed ||
                    status == HgLib.HgFileStatus.Renamed)
                {
                    addList.Add(name);
                }
            }

            if (addList.Count > 0)
            {
                RevertDialog(addList.ToArray());
            }
        }

        private void Exec_icmdHgAnnotate(object sender, EventArgs e)
        {
          string fileName = GetSingleSelectedFileName();
          HgAnnotateDlg(fileName);
        }
        
        public void HgAnnotateDlg(string fileName)
        {

            StoreSolution();

            if (fileName != String.Empty)
            {
                HgLib.HgFileStatus status = this.sccService.GetFileStatus(fileName);
                if (status == HgLib.HgFileStatus.Renamed)
                {
                    // get original filename
                    string orgName = HgLib.Hg.GetRenamedFileOriginalName(fileName);
                    if(orgName != string.Empty)
                        HgLib.TortoiseHg.AnnotateDialog(orgName);
                }
                
                if (status != HgLib.HgFileStatus.Uncontrolled &&
                    status != HgLib.HgFileStatus.Ignored)
                {
                    HgLib.TortoiseHg.AnnotateDialog(fileName);
                }
            }
        }

        // The function can be used to bring back the provider's toolwindow if it was previously closed
        private void Exec_icmdViewToolWindow(object sender, EventArgs e)
        {
            MsVsShell.ToolWindowPane window = this.FindToolWindow(typeof(HgPendingChangesToolWindow), 0, true);
            IVsWindowFrame windowFrame = null;
            if (window != null && window.Frame != null)
            {
                windowFrame = (IVsWindowFrame)window.Frame;
            }
            if (windowFrame != null)
            {
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
        }

        private void Exec_icmdToolWindowToolbarCommand(object sender, EventArgs e)
        {
            HgPendingChangesToolWindow window = (HgPendingChangesToolWindow)this.FindToolWindow(typeof(HgPendingChangesToolWindow), 0, true);

            if (window != null)
            {
                window.ToolWindowToolbarCommand();
            }
        }

        #endregion
    }
}
