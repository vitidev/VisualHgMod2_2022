using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Windows.Forms;
using HgLib;

namespace VisualHg
{
    public partial class SccProviderService
    {
        //--------------------------------------------------------------------------------
        // IVsSccManager2 specific functions
        //--------------------------------------------------------------------------------
        #region IVsSccManager2 interface functions

        public int BrowseForProject(out string pbstrDirectory, out int pfOK)
        {
            // Obsolete method
            pbstrDirectory = null;
            pfOK = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int CancelAfterBrowseForProject()
        {
            // Obsolete method
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Returns whether the source control provider is fully installed
        /// </summary>
        public int IsInstalled(out int pbInstalled)
        {
            // All source control packages should always return S_OK and set pbInstalled to nonzero
            pbInstalled = 1;
            return VSConstants.S_OK;
        }

        ImageMapper _statusImages = new ImageMapper();
        uint _baseIndex;
        ImageList _glyphList;
        /// <summary>
        /// Called by the IDE to get a custom glyph image list for source control status.
        /// </summary>
        /// <param name="BaseIndex">[in] Value to add when returning glyph index.</param>
        /// <param name="pdwImageListHandle">[out] Handle to the custom image list.</param>
        /// <returns>handle of an image list</returns>
        public int GetCustomGlyphList(uint BaseIndex, out uint pdwImageListHandle)
        {
            // We give VS all our custom glyphs from baseindex upwards
            if (_glyphList == null)
            {
                _baseIndex = BaseIndex;
                _glyphList = _statusImages.CreateStatusImageList();
            }
            pdwImageListHandle = unchecked((uint)_glyphList.Handle);

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Provide source control icons for the specified files and returns scc status of files
        /// </summary>
        /// <returns>The method returns S_OK if at least one of the files is controlled, S_FALSE if none of them are</returns>
        public int GetSccGlyph([InAttribute] int cFiles, [InAttribute] string[] rgpszFullPaths, [OutAttribute] VsStateIcon[] rgsiGlyphs, [OutAttribute] uint[] rgdwSccStatus)
        {
            if (rgpszFullPaths[0] == null)
                return 0;

            // Return the icons and the status. While the status is a combination a flags, we'll return just values 
            // with one bit set, to make life easier for GetSccGlyphsFromStatus
            HgLib.HgFileStatus status = _sccStatusTracker.GetFileStatus(rgpszFullPaths[0]);
            if (rgdwSccStatus != null)
                rgdwSccStatus[0] = 1; //__SccStatus.SCC_STATUS_CONTROLLED; -> SCC_STATUS_CONTROLLED = 1
            switch (status)
            {
                // STATEICON_CHECKEDIN schloss
                // STATEICON_CHECKEDOUT roter haken
                // STATEICON_CHECKEDOUTEXCLUSIVE roter haken
                // STATEICON_CHECKEDOUTSHAREDOTHER männchen
                // STATEICON_DISABLED roter ring / durchgestrichen
                //  STATEICON_EDITABLE bleistift
                // STATEICON_EXCLUDEDFROMSCC einbahnstrasse
                // STATEICON_MAXINDEX nix
                // STATEICON_NOSTATEICON nix
                // STATEICON_ORPHANED blaue flagge
                // STATEICON_READONLY schloss

                // my states
                case HgLib.HgFileStatus.Clean:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 0);
                    break;

                case HgLib.HgFileStatus.Modified:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 1);
                    break;

                case HgLib.HgFileStatus.Added:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 2);
                    break;

                case HgLib.HgFileStatus.Renamed:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 3);
                    break;

                case HgLib.HgFileStatus.Copied:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 3); // no better icon 
                    break;

                case HgLib.HgFileStatus.Removed:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 1);
                    break;

                case HgLib.HgFileStatus.Ignored:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_BLANK;
                    break;

                case HgLib.HgFileStatus.Uncontrolled:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_BLANK;
                    break;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines the corresponding scc status glyph to display, given a combination of scc status flags
        /// </summary>
        public int GetSccGlyphFromStatus([InAttribute] uint dwSccStatus, [OutAttribute] VsStateIcon[] psiGlyph)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// One of the most important methods in a source control provider,
        /// is called by projects that are under source control when they are first
        /// opened to register project settings
        /// </summary>
        public int RegisterSccProject([InAttribute] IVsSccProject2 pscp2Project, [InAttribute] string pszSccProjectName, [InAttribute] string pszSccAuxPath, [InAttribute] string pszSccLocalPath, [InAttribute] string pszProvider)
        {
            Trace.WriteLine("RegisterSccProject");

            if (pscp2Project != null)
            {
                _sccStatusTracker.UpdateProject(pscp2Project);
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by projects registered with the source control portion of the environment
        /// before they are closed. 
        /// </summary>
        public int UnregisterSccProject([InAttribute] IVsSccProject2 pscp2Project)
        {
            return VSConstants.S_OK;
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsSccManagerTooltip specific functions
        //--------------------------------------------------------------------------------
        #region IVsSccManagerTooltip interface functions

        /// <summary>
        /// Called by solution explorer to provide tooltips for items. Returns a text describing the source control status of the item.
        /// </summary>
        public int GetGlyphTipText([InAttribute] IVsHierarchy phierHierarchy, [InAttribute] uint itemidNode, out string pbstrTooltipText)
        {
            // Initialize output parameters
            pbstrTooltipText = "";

            IList<string> files = SccProvider.GetNodeFiles(phierHierarchy, itemidNode);
            if (files.Count == 0)
            {
                return VSConstants.S_OK;
            }

            // Return the glyph text based on the first file of node (the master file)
            HgLib.HgFileStatus status = _sccStatusTracker.GetFileStatus(files[0]);
            switch (status)
            {
                // my states
                case HgLib.HgFileStatus.Clean:
                    pbstrTooltipText = "Clean";
                    break;

                case HgLib.HgFileStatus.Modified:
                    pbstrTooltipText = "Modified";
                    break;

                case HgLib.HgFileStatus.Added:
                    pbstrTooltipText = "Added";
                    break;

                case HgLib.HgFileStatus.Renamed:
                    pbstrTooltipText = "Renamed";
                    break;

                case HgLib.HgFileStatus.Removed:
                    pbstrTooltipText = "Removed";
                    break;

                case HgLib.HgFileStatus.Copied:
                    pbstrTooltipText = "Copied";
                    break;

                case HgLib.HgFileStatus.Ignored:
                    pbstrTooltipText = "Ignored";
                    break;

                case HgLib.HgFileStatus.Uncontrolled:
                    pbstrTooltipText = "Uncontrolled";
                    break;

                default:
                    pbstrTooltipText = string.Empty;
                    break;
            }

            if (pbstrTooltipText != string.Empty)
            {
                string root = HgProvider.FindRepositoryRoot(files[0]);
                string branchName = _sccStatusTracker.GetDirectoryBranch(root);

                pbstrTooltipText += " [" + branchName + "]";
            }

            return VSConstants.S_OK;
        }

        #endregion
    }
}
