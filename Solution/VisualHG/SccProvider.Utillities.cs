using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using EnvDTE;
using HgLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    partial class SccProvider
    {
        public string CurrentRootDirectory
        {
            get
            {
                var projectItem = GetSelectedItems().FirstOrDefault(x => x.pHier as IVsProject != null);

                if (!projectItem.Equals(default(VSITEMSELECTION)))
                {
                    var fileName = GetItemFileName(projectItem);

                    if (!String.IsNullOrEmpty(fileName))
                    {
                        return HgPath.FindRepositoryRoot(fileName);
                    }
                }

                return SolutionRootDirectory;
            }
        }

        public string SolutionRootDirectory
        {
            get
            {
                var root = HgPath.FindRepositoryRoot(SolutionFileName);

                // This is for WebPage projects. The solution file is not included inside the Hg root dir.
                if (String.IsNullOrEmpty(root) && LastSeenProjectDirectory != null)
                {
                    return HgPath.FindRepositoryRoot(LastSeenProjectDirectory);
                }

                return root;
            }
        }

        public string SolutionFileName
        {
            get
            {
                var solution = GetService(typeof(SVsSolution)) as IVsSolution;
                string solutionDirectory, solutionFile, solutionUserOptions;

                solution.GetSolutionInfo(out solutionDirectory, out solutionFile, out solutionUserOptions);

                return solutionFile;
            }
        }

        public string SelectedFile
        {
            get
            {
                var dte = GetService(typeof(SDTE)) as _DTE;
                var selectedItems = GetSelectedItems();

                if (selectedItems.Length == 1)
                {
                    return GetItemFileName(selectedItems[0]);
                }
                else if (dte != null && dte.ActiveDocument != null)
                {
                    return dte.ActiveDocument.FullName;
                }

                return "";
            }
        }

        public IEnumerable<IVsHierarchy> LoadedProjects
        {
            get
            {
                var solution = GetService(typeof(SVsSolution)) as IVsSolution;

                var options = (uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION;
                var typeGuid = new Guid();
                IEnumHierarchies projectEnumerator;

                ErrorHandler.ThrowOnFailure(solution.GetProjectEnum(options, ref typeGuid, out projectEnumerator));

                uint count = 0;
                var projectArray = new IVsHierarchy[1];

                while (ErrorHandler.Succeeded(projectEnumerator.Next(1, projectArray, out count)) && count == 1)
                {
                    yield return projectArray[0];
                }
            }
        }


        public string[] GetSelectedFiles(bool includeChildren)
        {
            var selectedFiles = new List<string>();
            
            foreach (var item in GetSelectedItems())
            {
                var project = item.pHier as IVsProject;

                if (project == null)
                {
                    selectedFiles.Add(SolutionFileName);
                }
                else if (!includeChildren)
                {
                    selectedFiles.Add(GetItemFileName(project, item.itemid));
                }
                else
                {
                    selectedFiles.AddRange
                       (GetProjectItemIds(item.pHier, item.itemid)
                        .Select(x => GetItemFileName(project, x)));
                }
            }

            return selectedFiles.Where(x => !String.IsNullOrEmpty(x)).ToArray();
        }


        public void SaveAllFiles()
        {
            var solution = GetService(typeof(IVsSolution)) as IVsSolution;
            var options = (uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty;
            
            solution.SaveSolutionElement(options, null, 0);
        }


        public void UpdateGlyphs(VSITEMSELECTION[] items)
        {
            foreach (var item in items)
            {
                UpdateGlyph(item);
            }
        }

        private void UpdateGlyph(VSITEMSELECTION item)
        {
            var project = item.pHier as IVsSccProject2;

            if (item.itemid == VSConstants.VSITEMID_ROOT)
            {
                UpdateRootGlyph(project);
            }
            else
            {
                UpdateItemGlyph(project, item.itemid);
            }
        }

        private void UpdateRootGlyph(IVsSccProject2 project)
        {
            if (project == null)
            {
                UpdateSolutionGlyph();
            }
            else
            {
                UpdateAllProjectItemsGlyphs(project);
            }
        }

        private void UpdateSolutionGlyph()
        {
            var hierarchy = GetService(typeof(SVsSolution)) as IVsHierarchy;
            var property = (int)__VSHPROPID.VSHPROPID_StateIconIndex;
            var glyph = visualHgService.GetStateIcon(SolutionFileName);

            hierarchy.SetProperty(VSConstants.VSITEMID_ROOT, property, glyph);
        }

        private void UpdateAllProjectItemsGlyphs(IVsSccProject2 project)
        {
            project.SccGlyphChanged(0, null, null, null);
        }

        private void UpdateItemGlyph(IVsSccProject2 project, uint itemId)
        {
            var fileName = GetItemFiles(project, itemId).FirstOrDefault();

            if (!String.IsNullOrEmpty(fileName))
            {
                var affectedItem = new[] { itemId };
                var glyph = new[] { visualHgService.GetStateIcon(fileName) };

                project.SccGlyphChanged(1, affectedItem, glyph, new uint[1]);
            }
        }


        public void UpdateMainWindowCaption(string branch)
        {
            var dte = GetService(typeof(SDTE)) as _DTE;

            if (dte == null || dte.MainWindow == null)
            {
                return;
            }

            var caption = dte.MainWindow.Caption;
            var additionalInfo = String.IsNullOrEmpty(branch) ? "" : String.Concat(" (", branch, ") ");

            var newCaption = Regex.Replace(caption, 
                @"^(?<Solution>[^\(]+)(?<AdditionalInfo> \(.+\))? (?<Application>- [^\(]+) (?<User>\(.+\)) ?(?<Instance>- .+)$",
                String.Concat("${Solution}", additionalInfo, "${Application} ${User} ${Instance}"));

            if (caption != newCaption)
            {
                SetWindowText((IntPtr)dte.MainWindow.HWnd, newCaption);
            }
        }


        private bool FileIsNotAdded(string fileName)
        {
            return FileStatusMatches(fileName, HgFileStatus.NotAdded);
        }

        private bool FileIsPending(string fileName)
        {
            return FileStatusMatches(fileName, HgFileStatus.Pending);
        }

        private bool SearchAnySelectedFileStatusMatches(HgFileStatus pattern)
        {
            return AnySelectedFileStatusMatches(pattern, Configuration.Global.SearchIncludingChildren);
        }

        private bool AnySelectedFileStatusMatches(HgFileStatus pattern, bool includeChildren)
        {
            if (includeChildren)
            {
                return GetSelectedItems().Any(x => ItemOrChildrenStatusMatches(x, pattern));
            }
            
            return GetSelectedItems().Any(x => ItemStatusMatches(x, pattern));
        }

        private bool ItemOrChildrenStatusMatches(VSITEMSELECTION item, HgFileStatus pattern)
        {
            if (ItemStatusMatches(item, pattern))
            {
                return true;
            }

            return AnyChildItemStatusMatches(item, pattern);
        }

        private bool AnyChildItemStatusMatches(VSITEMSELECTION item, HgFileStatus pattern)
        {
            var project = item.pHier as IVsProject;

            if (project == null)
            {
                return false;
            }

            return GetProjectItemIds(item.pHier, item.itemid).
                Any(x => ItemStatusMatches(x, project, pattern));
        }

        private bool ItemStatusMatches(VSITEMSELECTION item, HgFileStatus pattern)
        {
            var fileName = GetItemFileName(item);

            return FileStatusMatches(fileName, pattern);
        }

        private bool ItemStatusMatches(uint itemId, IVsProject project, HgFileStatus pattern)
        {
            var fileName = GetItemFileName(project, itemId);

            return FileStatusMatches(fileName, pattern);
        }

        private bool SelectedFileStatusMatches(HgFileStatus pattern)
        {
            return FileStatusMatches(SelectedFile, pattern);
        }

        private bool FileStatusMatches(string fileName, HgFileStatus pattern)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                return false;
            }

            if (HgPath.IsDirectory(fileName))
            {
                return false;
            }

            var status = visualHgService.GetFileStatus(fileName);

            return (status & pattern) > 0;
        }
        
 
        private VSITEMSELECTION[] GetSelectedItems()
        {
            var selectedItems = new List<VSITEMSELECTION>();

            var hierarchy = IntPtr.Zero;
            var selectionContainer = IntPtr.Zero;
            
            try
            {
                uint itemId;
                IVsMultiItemSelect multiSelect;

                var selectionMonitor = GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;
                ErrorHandler.ThrowOnFailure(selectionMonitor.GetCurrentSelection(out hierarchy, out itemId, out multiSelect, out selectionContainer));

                if (SingleItemSelected(itemId))
                {
                    selectedItems.Add(GetSelectedItem(hierarchy, itemId));
                }
                else if (multiSelect != null)
                {
                    selectedItems.AddRange(GetSelectedItems(multiSelect));
                }
            }
            finally
            {
                ReleasePtr(hierarchy);
                ReleasePtr(selectionContainer);
            }

            return selectedItems.ToArray();
        }

        private static bool SingleItemSelected(uint itemId)
        {
            return itemId != VSConstants.VSITEMID_SELECTION && itemId != VSConstants.VSITEMID_NIL;
        }

        private static VSITEMSELECTION GetSelectedItem(IntPtr hierarchyPtr, uint itemId)
        {
            var item = new VSITEMSELECTION { itemid = itemId };

            if (hierarchyPtr != IntPtr.Zero)
            {
                var hierarchy = (IVsHierarchy)Marshal.GetObjectForIUnknown(hierarchyPtr);
                item.pHier = hierarchy;
            }

            return item;
        }

        private static VSITEMSELECTION[] GetSelectedItems(IVsMultiItemSelect multiSelect)
        {
            var selectedItemsCount = GetSelectedItemsCount(multiSelect);
            var selectedItems = new VSITEMSELECTION[selectedItemsCount];

            if (selectedItemsCount > 0)
            {
                ErrorHandler.ThrowOnFailure(multiSelect.GetSelectedItems(0, selectedItemsCount, selectedItems));
            }
            
            return selectedItems;
        }

        private static uint GetSelectedItemsCount(IVsMultiItemSelect multiSelect)
        {
            uint selectedItemsCount;
            int isSingleHierarchy;
            
            ErrorHandler.ThrowOnFailure(multiSelect.GetSelectionInfo(out selectedItemsCount, out isSingleHierarchy));
            
            return selectedItemsCount;
        }

        private static void ReleasePtr(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.Release(ptr);
            }
        }

        
        private string GetItemFileName(VSITEMSELECTION item)
        {
            var project = item.pHier as IVsProject;

            if (project == null)
            {
                return SolutionFileName;
            }

            return GetItemFileName(project, item.itemid);
        }
       
        private string GetItemFileName(IVsProject project, uint itemId)
        {
            string fileName;

            project.GetMkDocument(itemId, out fileName);

            return fileName;
        }


        public static string GetDirectoryName(IVsHierarchy hierarchy)
        {
            object name = null;

            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir, out name);

            var directory = name as string;

            return !String.IsNullOrEmpty(directory) ? GetNormalizedFullPath(directory) : "";
        }

        private static string GetNormalizedFullPath(string path)
        {
            path = Path.GetFullPath(path);

            if (path.Length >= 2 && path[1] == ':')
            {
                var driveLetter = path[0];

                if ((driveLetter >= 'a') && (driveLetter <= 'z'))
                {
                    path = driveLetter.ToString().ToUpperInvariant() + path.Substring(1);
                }

                var r = path.TrimEnd('\\');

                if (r.Length > 3)
                {
                    return r;
                }

                return path.Substring(0, 3);
            }
            else if (path.StartsWith(@"\\"))
            {
                var root = Path.GetPathRoot(path).ToLowerInvariant();

                if (!path.StartsWith(root))
                {
                    path = root + path.Substring(root.Length).TrimEnd('\\');
                }
            }

            return path.TrimEnd('\\');
        }


        public static string[] GetProjectFiles(IVsSccProject2 project)
        {
            return GetProjectFiles(project, VSConstants.VSITEMID_ROOT);
        }

        private static string[] GetProjectFiles(IVsSccProject2 project, uint itemId)
        {
            var itemIds = GetProjectItemIds(project as IVsHierarchy, itemId);

            return itemIds.SelectMany(x => GetItemFiles(project, x)).ToArray();
        }

        private static IEnumerable<uint> GetProjectItemIds(IVsHierarchy hierarchy, uint itemId)
        {
            var items = new Queue<uint>();

            if (hierarchy != null)
            {
                items.Enqueue(itemId);
            }

            while (items.Count > 0)
            {
                uint item = items.Dequeue();

                yield return item;

                item = GetItemFirstChild(hierarchy, item);
                
                if (item == VSConstants.VSITEMID_NIL)
                {
                    continue;
                }

                if (ItemHasChildren(hierarchy, item))
                {
                    items.Enqueue(item);
                }
                else
                {
                    yield return item;
                }

                while (TryGetItemNextSibling(hierarchy, item, out item))
                {
                    if (item == VSConstants.VSITEMID_NIL)
                    {
                        break;
                    }

                    if (ItemHasChildren(hierarchy, item))
                    {
                        items.Enqueue(item);
                    }
                    else
                    {
                        yield return item;
                    }
                }
            }
        }

        private static uint GetItemFirstChild(IVsHierarchy hierarchy, uint itemId)
        {
            uint firstChild;

            if (TryGetItemNextId(hierarchy, itemId, __VSHPROPID.VSHPROPID_FirstChild, out firstChild))
            {
                return firstChild;
            }

            return VSConstants.VSITEMID_NIL;
        }

        private static bool TryGetItemNextSibling(IVsHierarchy hierarchy, uint itemId, out uint siblingItemId)
        {
            return TryGetItemNextId(hierarchy, itemId, __VSHPROPID.VSHPROPID_NextSibling, out siblingItemId);
        }

        private static bool ItemHasChildren(IVsHierarchy hierarchy, uint itemId)
        {
            return ItemIsExpandable(hierarchy, itemId) || ItemIsContainer(hierarchy, itemId);
        }

        private static bool ItemIsExpandable(IVsHierarchy hierarchy, uint itemId)
        {
            object property = null;

            if (TryGetItemProperty(hierarchy, itemId, __VSHPROPID.VSHPROPID_Expandable, out property))
            {
                return (int)property != 0;
            }

            return false;
        }

        private static bool ItemIsContainer(IVsHierarchy hierarchy, uint itemId)
        {
            object property = null;

            if (TryGetItemProperty(hierarchy, itemId, __VSHPROPID2.VSHPROPID_Container, out property))
            {
                return (bool)property;
            }

            return false;
        }

        private static bool TryGetItemNextId(IVsHierarchy hierarchy, uint itemId, __VSHPROPID property, out uint nextId)
        {
            object value = null;
            
            var result = TryGetItemProperty(hierarchy, itemId, property, out value);

            nextId = result ? (uint)(int)value : VSConstants.VSITEMID_NIL;
            
            return result;
        }

        private static bool TryGetItemProperty(IVsHierarchy hierarchy, uint itemId, __VSHPROPID2 property, out object value)
        {
            return ErrorHandler.Succeeded(hierarchy.GetProperty(itemId, (int)property, out value));
        }

        private static bool TryGetItemProperty(IVsHierarchy hierarchy, uint itemId, __VSHPROPID property, out object value)
        {
            return ErrorHandler.Succeeded(hierarchy.GetProperty(itemId, (int)property, out value));
        }

        
        public static string[] GetItemFiles(IVsHierarchy hierarchy, uint itemId)
        {
            var project = hierarchy as IVsSccProject2;

            if (project != null)
            {
                return GetItemFiles(project, itemId);
            }

            return new string[0];
        }

        private static string[] GetItemFiles(IVsSccProject2 project, uint itemId)
        {
            var itemFiles = new List<string>();

            var files = new CALPOLESTR[1];
            var flags = new CADWORD[1];

            if (ErrorHandler.Succeeded(project.GetSccFiles(itemId, files, flags)))
            {
                var fileNames = GetFileNames(files[0]);

                for (int i = 0; i < files[0].cElems; i++)
                {
                    itemFiles.Add(fileNames[i]);

                    if (HasSpecialFiles(flags, i))
                    {
                        itemFiles.AddRange(GetSpecialFiles(project, itemId, fileNames[i]));
                    }
                }
            }

            return itemFiles.ToArray();
        }

        private static string[] GetSpecialFiles(IVsSccProject2 project, uint itemId, string fileName)
        {
            var specialFiles = new CALPOLESTR[1];
            var specialFlags = new CADWORD[1];

            if (ErrorHandler.Succeeded(project.GetSccSpecialFiles(itemId, fileName, specialFiles, specialFlags)))
            {
                return GetFileNames(specialFiles[0]);
            }

            return new string[0];
        }

        private static string[] GetFileNames(CALPOLESTR array)
        {
            var files = new string[array.cElems];

            for (int i = 0; i < files.Length; i++)
            {
                var pathPtr = Marshal.ReadIntPtr(array.pElems, i * IntPtr.Size);
                
                files[i] = Marshal.PtrToStringUni(pathPtr);

                Marshal.FreeCoTaskMem(pathPtr);
            }
            
            if (array.pElems != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(array.pElems);
            }

            return files;
        }

        private static bool HasSpecialFiles(CADWORD[] flags, int i)
        {
            if (flags[0].cElems > 0)
            {
                return Marshal.ReadInt32(flags[0].pElems, i) != 0;
            }

            return false;
        }



        [DllImport("user32.dll")]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);
    }
}