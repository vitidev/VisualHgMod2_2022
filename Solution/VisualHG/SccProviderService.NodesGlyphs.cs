using System;
using System.Windows.Forms;
using HgLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    public partial class SccProviderService
    {
        private uint iconBaseIndex;
        private ImageList statusImageList;

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
            if (statusImageList==null)
            {
                statusImageList = ImageMapper.CreateStatusImageList(Configuration.Global.StatusImageFileName);
            }

            iconBaseIndex = BaseIndex;
                
            pdwImageListHandle = unchecked((uint)statusImageList.Handle);

            return VSConstants.S_OK;
        }

        public int GetSccGlyph(int count, string[] fileNames, VsStateIcon[] icons, uint[] statuses)
        {
            if (count == 0 || String.IsNullOrEmpty(fileNames[0]))
            {
                return VSConstants.S_OK;
            }

            var status = Repository.GetFileStatus(fileNames[0]);
            var iconIndex = ImageMapper.GetStatusIconIndex(status);

            icons[0] = (VsStateIcon)(iconBaseIndex + iconIndex);

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

            var fileName = files[0];
            var status = Repository.GetFileStatus(fileName).ToString();
            var branch = Repository.GetBranch(fileName);

            if (!String.IsNullOrEmpty(status))
            {
                pbstrTooltipText = status;
            }

            if (!String.IsNullOrEmpty(branch))
            {
                pbstrTooltipText += " (" + branch + ")";
            }

            return VSConstants.S_OK;
        }
    }
}