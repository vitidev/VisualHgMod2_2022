using System;
using System.Windows.Forms;
using HgLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    public partial class SccProviderService
    {
        private uint _baseIndex;
        private ImageList _glyphList;
        private ImageMapper _statusImages = new ImageMapper();

        public int BrowseForProject(out string pbstrDirectory, out int pfOK)
        {
            pbstrDirectory = null;
            pfOK = 0;

            return VSConstants.E_NOTIMPL;
        }

        public int CancelAfterBrowseForProject()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int IsInstalled(out int pbInstalled)
        {
            pbInstalled = 1;
            return VSConstants.S_OK;
        }

        
        public int GetCustomGlyphList(uint BaseIndex, out uint pdwImageListHandle)
        {
            if (_glyphList == null)
            {
                _baseIndex = BaseIndex;
                _glyphList = _statusImages.CreateStatusImageList();
            }

            pdwImageListHandle = unchecked((uint)_glyphList.Handle);

            return VSConstants.S_OK;
        }

        public int GetSccGlyph(int cFiles, string[] rgpszFullPaths, VsStateIcon[] rgsiGlyphs, uint[] rgdwSccStatus)
        {
            if (rgpszFullPaths[0] == null)
            {
                return VSConstants.S_OK;
            }

            if (rgdwSccStatus != null)
            {
                rgdwSccStatus[0] = (uint)__SccStatus.SCC_STATUS_CONTROLLED;
            }
            
            var status = Repository.GetFileStatus(rgpszFullPaths[0]);

            switch (status)
            {
                case HgFileStatus.Clean:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 0);
                    break;

                case HgFileStatus.Modified:
                case HgFileStatus.Removed:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 1);
                    break;

                case HgFileStatus.Added:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 2);
                    break;

                case HgFileStatus.Renamed:
                case HgFileStatus.Copied:
                    rgsiGlyphs[0] = (VsStateIcon)(_baseIndex + 3);
                    break;

                case HgFileStatus.Ignored:
                case HgFileStatus.Uncontrolled:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_BLANK;
                    break;
            }

            return VSConstants.S_OK;
        }

        public int GetSccGlyphFromStatus(uint dwSccStatus, VsStateIcon[] psiGlyph)
        {
            return VSConstants.S_OK;
        }

        public int RegisterSccProject(IVsSccProject2 pscp2Project, string pszSccProjectName, string pszSccAuxPath, string pszSccLocalPath, string pszProvider)
        {
            if (pscp2Project != null)
            {
                Repository.UpdateProject(pscp2Project);
            }

            return VSConstants.S_OK;
        }

        public int UnregisterSccProject(IVsSccProject2 pscp2Project)
        {
            return VSConstants.S_OK;
        }


        public int GetGlyphTipText(IVsHierarchy phierHierarchy, uint itemidNode, out string pbstrTooltipText)
        {
            pbstrTooltipText = "";

            var files = SccProvider.GetNodeFiles(phierHierarchy, itemidNode);
            
            if (files.Count == 0)
            {
                return VSConstants.S_OK;
            }

            var status = Repository.GetFileStatus(files[0]);
            var root = HgProvider.FindRepositoryRoot(files[0]);
            var branch = Repository.GetDirectoryBranch(root);


            var statusName = Enum.IsDefined(typeof(HgFileStatus), status) ? status.ToString() : "";

            if (!String.IsNullOrEmpty(statusName) && !String.IsNullOrEmpty(branch))
            {
                statusName += " (" + branch + ")";
            }

            pbstrTooltipText = statusName;

            return VSConstants.S_OK;
        }
    }
}
