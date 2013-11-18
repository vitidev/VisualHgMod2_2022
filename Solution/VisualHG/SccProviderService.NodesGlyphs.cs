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

        public int GetSccGlyph(int count, string[] fileNames, VsStateIcon[] icons, uint[] statuses)
        {
            if (count == 0)
            {
                return VSConstants.S_OK;
            }

            var status = Repository.GetFileStatus(fileNames[0]);
            var imageIndex = ImageMapper.GetStatusIconIndex(status);

            icons[0] = (VsStateIcon)(_baseIndex + imageIndex);

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

            var files = SccProvider.GetItemFiles(phierHierarchy, itemidNode);
            
            if (files.Length == 0)
            {
                return VSConstants.S_OK;
            }

            var status = Repository.GetFileStatus(files[0]);
            var statusName = Enum.IsDefined(typeof(HgFileStatus), status) ? status.ToString() : "";

            var root = HgPath.FindRepositoryRoot(files[0]);
            var branch = Repository.GetDirectoryBranch(root);

            if (!String.IsNullOrEmpty(statusName) && !String.IsNullOrEmpty(branch))
            {
                statusName += " (" + branch + ")";
            }

            pbstrTooltipText = statusName;

            return VSConstants.S_OK;
        }
    }
}