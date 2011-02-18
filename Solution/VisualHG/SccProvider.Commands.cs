using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel.Design;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;
using MsVsShell = Microsoft.VisualStudio.Shell;
using System.Windows.Forms;

namespace VisualHG
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
                cmdf = cmdf | OLECMDF.OLECMDF_INVISIBLE;

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

                case CommandId.icmdViewToolWindow:
                case CommandId.icmdToolWindowToolbarCommand:
                    // These commmands are always enabled when the provider is active
                    cmdf = OLECMDF.OLECMDF_INVISIBLE;
                    cmdf &= ~OLECMDF.OLECMDF_ENABLED;
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

        OLECMDF QueryStatus_icmdHgCommitSelected()
        {
            OLECMDF cmdf = OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE;

            List<string> array = GetSelectedFileNameArray(true);
            foreach (string filename in array)
            {
                HGLib.SourceControlStatus status = this.sccService.GetFileStatus(filename);
                if (status != HGLib.SourceControlStatus.scsUncontrolled &&
                    status != HGLib.SourceControlStatus.scsClean &&
                    status != HGLib.SourceControlStatus.scsIgnored)
                {
                    cmdf = OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                    break;
                }
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
                HGLib.SourceControlStatus status = this.sccService.GetFileStatus(filename);
                if (status != HGLib.SourceControlStatus.scsUncontrolled &&
                    status != HGLib.SourceControlStatus.scsIgnored &&
                    status != HGLib.SourceControlStatus.scsAdded &&
                    status != HGLib.SourceControlStatus.scsRenamed &&
                    status != HGLib.SourceControlStatus.scsCopied)
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
                HGLib.SourceControlStatus status = this.sccService.GetFileStatus(filename);
                if (status != HGLib.SourceControlStatus.scsUncontrolled &&
                    status != HGLib.SourceControlStatus.scsAdded &&
                    status != HGLib.SourceControlStatus.scsIgnored &&
                    status != HGLib.SourceControlStatus.scsClean)
                    return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;

            }

            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE;
        }

        OLECMDF QueryStatus_icmdHgRevert()
        {
            string filename = GetSingleSelectedFileName();
            if (filename != String.Empty)
            {
                HGLib.SourceControlStatus status = this.sccService.GetFileStatus(filename);
                if (status != HGLib.SourceControlStatus.scsUncontrolled &&
                    status != HGLib.SourceControlStatus.scsClean)
                    return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;

            }

            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE;
        }

        OLECMDF QueryStatus_icmdHgAnnotate()
        {
            string filename = GetSingleSelectedFileName();
            if (filename != String.Empty)
            {
                HGLib.SourceControlStatus status = this.sccService.GetFileStatus(filename);
                if (status != HGLib.SourceControlStatus.scsUncontrolled &&
                    status != HGLib.SourceControlStatus.scsIgnored &&
                    status != HGLib.SourceControlStatus.scsAdded &&
                    status != HGLib.SourceControlStatus.scsRenamed &&
                    status != HGLib.SourceControlStatus.scsCopied)
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
                HGLib.HGTK.CommitDialog(root);
            else
                PromptSolutionNotControlled();
        }

        private void Exec_icmdHgCommitSelected(object sender, EventArgs e)
        {
            StoreSolution();

            List<string> array = GetSelectedFileNameArray(true);
            List<string> commitList = new List<string>();
            foreach (string name in array)
            {
                HGLib.SourceControlStatus status = this.sccService.GetFileStatus(name);
                if (status != HGLib.SourceControlStatus.scsUncontrolled &&
                    status != HGLib.SourceControlStatus.scsClean &&
                    status != HGLib.SourceControlStatus.scsIgnored)
                {
                    commitList.Add(name);
                }
            }

            if (commitList.Count > 0)
            {
                HGLib.HGTK.CommitDialog(commitList.ToArray());
            }
        }

        private void Exec_icmdHgHistoryRoot(object sender, EventArgs e)
        {
            StoreSolution();

            string root = GetRootDirectory();

            if (root != null && root != String.Empty)
                HGLib.HGTK.RepoBrowserDialog(root);
            else
                PromptSolutionNotControlled();
        }

        private void Exec_icmdHgHistorySelected(object sender, EventArgs e)
        {
            StoreSolution();

            string fileName = GetSingleSelectedFileName();
            if (fileName != string.Empty)
            {
                HGLib.SourceControlStatus status = this.sccService.GetFileStatus(fileName);
                if (status != HGLib.SourceControlStatus.scsUncontrolled &&
                    status != HGLib.SourceControlStatus.scsAdded &&
                    status != HGLib.SourceControlStatus.scsIgnored)
                {
                    HGLib.HGTK.LogDialog(fileName); 
                }
            }
        }

        private void Exec_icmdHgStatus(object sender, EventArgs e)
        {
            StoreSolution();

            string root = GetRootDirectory();
            if (root != string.Empty)
                HGLib.HGTK.StatusDialog(root);
            else
                PromptSolutionNotControlled();

        }

        private void Exec_icmdHgDiff(object sender, EventArgs e)
        {
            StoreSolution();

            string fileName = GetSingleSelectedFileName();

            if (fileName != String.Empty)
            {
                string versionedFile = fileName;

                HGLib.SourceControlStatus status = this.sccService.GetFileStatus(fileName);
                if (status != HGLib.SourceControlStatus.scsUncontrolled &&
                    status != HGLib.SourceControlStatus.scsAdded &&
                    status != HGLib.SourceControlStatus.scsIgnored)
                {
                    if (status == HGLib.SourceControlStatus.scsRenamed ||
                        status == HGLib.SourceControlStatus.scsCopied)
                    {
                        versionedFile = HGLib.HG.GetOriginalOfRenamedFile(fileName); 
                    }

                    if (versionedFile != null)
                    { 
                        try
                        { 
                            HGLib.HGTK.DiffDialog(versionedFile, fileName, Configuration.Global.ExternalDiffToolCommandMask);
                        }
                        catch
                        {
                            if (Configuration.Global.ExternalDiffToolCommandMask != string.Empty)
                                MessageBox.Show("The DiffTool raised an error\nPlease check your command mask:\n\n" + Configuration.Global.ExternalDiffToolCommandMask,
                                                "VisualHG",
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
                HGLib.HGTK.SyncDialog(root);
            else
                PromptSolutionNotControlled();
        }

        private void Exec_icmdHgUpdateToRevision(object sender, EventArgs e)
        {
            StoreSolution();

            string root = GetRootDirectory();
            if (root != string.Empty)
                HGLib.HGTK.UpdateDialog(root);
            else
                PromptSolutionNotControlled();
        }

        private void Exec_icmdHgRevert(object sender, EventArgs e)
        {
            StoreSolution();

            string fileName = GetSingleSelectedFileName();
            if (fileName != String.Empty)
            {
                HGLib.SourceControlStatus status = this.sccService.GetFileStatus(fileName);
                if (status != HGLib.SourceControlStatus.scsUncontrolled &&
                    status != HGLib.SourceControlStatus.scsClean)
                {
                    HGLib.HGTK.RevertDialog(fileName);
                }
            }
        }

        private void Exec_icmdHgAnnotate(object sender, EventArgs e)
        {
            StoreSolution();

            string fileName = GetSingleSelectedFileName();
            if (fileName != String.Empty)
            {
                HGLib.SourceControlStatus status = this.sccService.GetFileStatus(fileName);
                if (status == HGLib.SourceControlStatus.scsRenamed)
                {
                    // get original filename
                    string orgName = HGLib.HG.GetOriginalOfRenamedFile(fileName);
                    if(orgName != string.Empty)
                        HGLib.HGTK.AnnotateDialog(orgName);
                }
                
                if (status != HGLib.SourceControlStatus.scsUncontrolled &&
                    status != HGLib.SourceControlStatus.scsIgnored)
                {
                    HGLib.HGTK.AnnotateDialog(fileName);
                }
            }
        }


        // The function can be used to bring back the provider's toolwindow if it was previously closed
        private void Exec_icmdViewToolWindow(object sender, EventArgs e)
        {
            MsVsShell.ToolWindowPane window = this.FindToolWindow(typeof(HGPendingChangesToolWindow), 0, true);
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
            HGPendingChangesToolWindow window = (HGPendingChangesToolWindow)this.FindToolWindow(typeof(HGPendingChangesToolWindow), 0, true);

            if (window != null)
            {
                window.ToolWindowToolbarCommand();
            }
        }

        #endregion
    }
}
