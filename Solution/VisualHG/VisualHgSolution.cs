using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using HgLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    public static class VisualHgSolution
    {
        public static string LastSeenProjectDirectory { get; set; }

        public static bool IsUnderSourceControl => !string.IsNullOrEmpty(SolutionRootDirectory);

        public static string CurrentRootDirectory
        {
            get
            {
                var projectItem = GetSelectedItems().FirstOrDefault(x => x.pHier as IVsProject != null);

                if (!projectItem.Equals(default(VSITEMSELECTION)))
                {
                    var fileName = GetItemFileName(projectItem);

                    if (!string.IsNullOrEmpty(fileName))
                        return HgPath.FindRepositoryRoot(fileName);
                }

                return SolutionRootDirectory;
            }
        }

        public static string SolutionRootDirectory
        {
            get
            {
                var root = HgPath.FindRepositoryRoot(SolutionFileName);

                // This is for WebPage projects. The solution file is not included inside the Hg root dir.
                if (string.IsNullOrEmpty(root) && !string.IsNullOrEmpty(LastSeenProjectDirectory))
                    return HgPath.FindRepositoryRoot(LastSeenProjectDirectory);

                return root;
            }
        }

        public static string SolutionFileName
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));

                solution.GetSolutionInfo(out _, out var solutionFile, out _);

                return solutionFile;
            }
        }

        public static string SelectedFile
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var dte = Package.GetGlobalService(typeof(SDTE)) as _DTE;
                var selectedItems = GetSelectedItems();

                if (selectedItems.Length == 1)
                    return GetItemFileName(selectedItems[0]);

                if (dte?.ActiveDocument != null)
                {
                    return dte.ActiveDocument.FullName;
                }

                return "";
            }
        }

        public static IEnumerable<IVsHierarchy> LoadedProjects
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));

                var options = (uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION;
                var typeGuid = new Guid();

                ErrorHandler.ThrowOnFailure(solution.GetProjectEnum(options, ref typeGuid, out var projectEnumerator));

                var projectArray = new IVsHierarchy[1];

                while (ErrorHandler.Succeeded(projectEnumerator.Next(1, projectArray, out var count)) && count == 1)
                {
                    yield return projectArray[0];
                }
            }
        }


        public static IVsHierarchy FindProject(string fileName)
        {
            var itemId = VSConstants.VSITEMID_ROOT;

            return LoadedProjects.FirstOrDefault(x => GetItemFiles(x, itemId).Any(y => y == fileName));
        }

        public static IEnumerable<string> GetChildrenFiles(IVsHierarchy hierarchy)
        {
            return GetProjectItemIds(hierarchy, VSConstants.VSITEMID_ROOT)
                .Skip(1)
                .Select(x => GetItemFiles(hierarchy, x).FirstOrDefault());
        }

        public static string[] GetSelectedFiles(bool includeChildren)
        {
            var selectedFiles = new List<string>();

            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (var item in GetSelectedItems())
            {
                if (!(item.pHier is IVsProject project))
                    selectedFiles.Add(SolutionFileName);
                else if (!includeChildren)
                    selectedFiles.Add(GetItemFileName(project, item.itemid));
                else
                {
                    selectedFiles.AddRange
                    (GetProjectItemIds(item.pHier, item.itemid)
                        .Select(x => GetItemFileName(project, x)));
                }
            }

            return selectedFiles.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        }


        public static bool SearchAnySelectedFileStatusMatches(HgFileStatus pattern)
        {
            return AnySelectedFileStatusMatches(pattern, VisualHgOptions.Global.ProjectStatusIncludesChildren);
        }

        private static bool AnySelectedFileStatusMatches(HgFileStatus pattern, bool includeChildren)
        {
            if (includeChildren)
            {
                return GetSelectedItems().Any(x => ItemOrChildrenStatusMatches(x, pattern));
            }

            return GetSelectedItems().Any(x => ItemStatusMatches(x, pattern));
        }

        private static bool ItemOrChildrenStatusMatches(VSITEMSELECTION item, HgFileStatus pattern)
        {
            if (ItemStatusMatches(item, pattern))
            {
                return true;
            }

            return AnyChildItemStatusMatches(item, pattern);
        }

        private static bool AnyChildItemStatusMatches(VSITEMSELECTION item, HgFileStatus pattern)
        {
            if (!(item.pHier is IVsProject project))
                return false;

            return GetProjectItemIds(item.pHier, item.itemid).Any(x => ItemStatusMatches(x, project, pattern));
        }

        private static bool ItemStatusMatches(VSITEMSELECTION item, HgFileStatus pattern)
        {
            var fileName = GetItemFileName(item);

            return VisualHgFileStatus.Matches(fileName, pattern);
        }

        private static bool ItemStatusMatches(uint itemId, IVsProject project, HgFileStatus pattern)
        {
            var fileName = GetItemFileName(project, itemId);

            return VisualHgFileStatus.Matches(fileName, pattern);
        }

        public static bool SelectedFileStatusMatches(HgFileStatus pattern)
        {
            return VisualHgFileStatus.Matches(SelectedFile, pattern);
        }


        private static VSITEMSELECTION[] GetSelectedItems()
        {
            var selectedItems = new List<VSITEMSELECTION>();

            var hierarchy = IntPtr.Zero;
            var selectionContainer = IntPtr.Zero;

            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var selectionMonitor = (IVsMonitorSelection)Package.GetGlobalService(typeof(IVsMonitorSelection));
                ErrorHandler.ThrowOnFailure(selectionMonitor.GetCurrentSelection(out hierarchy, out var itemId,
                    out var multiSelect, out selectionContainer));

                if (SingleItemSelected(itemId))
                    selectedItems.Add(GetSelectedItem(hierarchy, itemId));
                else if (multiSelect != null) selectedItems.AddRange(GetSelectedItems(multiSelect));
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
            var item = new VSITEMSELECTION {itemid = itemId};

            if (hierarchyPtr != IntPtr.Zero)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var hierarchy = (IVsHierarchy)Marshal.GetObjectForIUnknown(hierarchyPtr);
                item.pHier = hierarchy;
            }

            return item;
        }

        private static VSITEMSELECTION[] GetSelectedItems(IVsMultiItemSelect multiSelect)
        {
            var selectedItemsCount = GetSelectedItemsCount(multiSelect);
            var selectedItems = new VSITEMSELECTION[selectedItemsCount];

            ThreadHelper.ThrowIfNotOnUIThread();

            if (selectedItemsCount > 0)
                ErrorHandler.ThrowOnFailure(multiSelect.GetSelectedItems(0, selectedItemsCount, selectedItems));

            return selectedItems;
        }

        private static uint GetSelectedItemsCount(IVsMultiItemSelect multiSelect)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ErrorHandler.ThrowOnFailure(multiSelect.GetSelectionInfo(out var selectedItemsCount, out _));

            return selectedItemsCount;
        }

        private static void ReleasePtr(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero) Marshal.Release(ptr);
        }


        private static string GetItemFileName(VSITEMSELECTION item)
        {
            if (!(item.pHier is IVsProject project))
                return SolutionFileName;

            return GetItemFileName(project, item.itemid);
        }

        private static string GetItemFileName(IVsProject project, uint itemId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            project.GetMkDocument(itemId, out var fileName);

            return fileName;
        }


        public static string GetDirectoryName(IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir, out var name);

            var directory = name as string;

            return !string.IsNullOrEmpty(directory) ? GetNormalizedFullPath(directory) : "";
        }

        private static string GetNormalizedFullPath(string path)
        {
            path = Path.GetFullPath(path);

            if (path.Length >= 2 && path[1] == ':')
            {
                var driveLetter = path[0];

                if (driveLetter >= 'a' && driveLetter <= 'z')
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

            if (path.StartsWith(@"\\"))
            {
                var root = Path.GetPathRoot(path).ToLowerInvariant();

                if (!path.StartsWith(root))
                {
                    path = root + path.Substring(root.Length).TrimEnd('\\');
                }
            }

            return path.TrimEnd('\\');
        }


        public static string[] GetProjectFiles(IVsHierarchy hierarchy)
        {
            if (!(hierarchy is IVsSccProject2 project))
                return new string[0];

            return GetProjectFiles(project);
        }

        public static string[] GetProjectFiles(IVsSccProject2 project)
        {
            return GetProjectFiles(project, VSConstants.VSITEMID_ROOT);
        }

        private static string[] GetProjectFiles(IVsSccProject2 project, uint itemId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var itemIds = GetProjectItemIds(project as IVsHierarchy, itemId);

            return itemIds.SelectMany(x => GetItemFiles(project, x)).ToArray();
        }


        private static IEnumerable<uint> GetProjectItemIds(IVsHierarchy hierarchy, uint itemId)
        {
            var items = new Queue<uint>();

            if (hierarchy != null) items.Enqueue(itemId);

            while (items.Count > 0)
            {
                var item = items.Dequeue();

                yield return item;

                item = GetItemFirstChild(hierarchy, item);

                if (item == VSConstants.VSITEMID_NIL)
                    continue;

                if (ItemHasChildren(hierarchy, item))
                    items.Enqueue(item);
                else
                    yield return item;

                while (TryGetItemNextSibling(hierarchy, item, out item))
                {
                    if (item == VSConstants.VSITEMID_NIL)
                        break;

                    if (ItemHasChildren(hierarchy, item))
                        items.Enqueue(item);
                    else
                        yield return item;
                }
            }
        }

        private static uint GetItemFirstChild(IVsHierarchy hierarchy, uint itemId)
        {
            if (TryGetItemNextId(hierarchy, itemId, __VSHPROPID.VSHPROPID_FirstChild, out var firstChild))
                return firstChild;

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
            if (TryGetItemProperty(hierarchy, itemId, __VSHPROPID.VSHPROPID_Expandable, out var property))
            {
                if (property is bool value)
                    return value;

                return (int)property != 0;
            }

            return false;
        }

        private static bool ItemIsContainer(IVsHierarchy hierarchy, uint itemId)
        {
            if (TryGetItemProperty(hierarchy, itemId, __VSHPROPID2.VSHPROPID_Container, out var property))
                return (bool)property;

            return false;
        }

        private static bool TryGetItemNextId(IVsHierarchy hierarchy, uint itemId, __VSHPROPID property, out uint nextId)
        {
            var result = TryGetItemProperty(hierarchy, itemId, property, out var value);

            nextId = result ? (uint)(int)value : VSConstants.VSITEMID_NIL;

            return result;
        }

        private static bool TryGetItemProperty(IVsHierarchy hierarchy, uint itemId, __VSHPROPID2 property,
            out object value)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ErrorHandler.Succeeded(hierarchy.GetProperty(itemId, (int)property, out value));
        }

        private static bool TryGetItemProperty(IVsHierarchy hierarchy, uint itemId, __VSHPROPID property,
            out object value)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ErrorHandler.Succeeded(hierarchy.GetProperty(itemId, (int)property, out value));
        }


        public static string[] GetItemFiles(IVsHierarchy hierarchy, uint itemId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (hierarchy is IVsSccProject2 project)
                return GetItemFiles(project, itemId);

            return new string[0];
        }

        public static string[] GetItemFiles(IVsSccProject2 project, uint itemId)
        {
            var itemFiles = new List<string>();

            var files = new CALPOLESTR[1];
            var flags = new CADWORD[1];

            ThreadHelper.ThrowIfNotOnUIThread();

            if (ErrorHandler.Succeeded(project.GetSccFiles(itemId, files, flags)))
            {
                var fileNames = GetFileNames(files[0]);

                for (var i = 0; i < files[0].cElems; i++)
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

            ThreadHelper.ThrowIfNotOnUIThread();

            if (ErrorHandler.Succeeded(project.GetSccSpecialFiles(itemId, fileName, specialFiles, specialFlags)))
            {
                return GetFileNames(specialFiles[0]);
            }

            return new string[0];
        }

        private static string[] GetFileNames(CALPOLESTR array)
        {
            var files = new string[array.cElems];

            for (var i = 0; i < files.Length; i++)
            {
                var pathPtr = Marshal.ReadIntPtr(array.pElems, i * IntPtr.Size);

                files[i] = Marshal.PtrToStringUni(pathPtr);

                Marshal.FreeCoTaskMem(pathPtr);
            }

            if (array.pElems != IntPtr.Zero) Marshal.FreeCoTaskMem(array.pElems);

            return files;
        }

        private static bool HasSpecialFiles(CADWORD[] flags, int i)
        {
            if (flags[0].cElems > 0)
                return Marshal.ReadInt32(flags[0].pElems, i) != 0;

            return false;
        }
    }
}