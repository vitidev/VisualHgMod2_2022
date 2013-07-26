using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;

namespace VisualHg
{
    /// <summary>
    ///  SccProvider Utillity Function
    /// </summary>
    partial class SccProvider
    {
        #region Source Control Utility Functions

        public void StoreSolution()
        {
            // store project and solution files to disk
            IVsSolution solution = (IVsSolution)GetService(typeof(IVsSolution));
            solution.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, null, 0);
        }

        /// <summary>
        /// Returns a list of controllable projects in the solution
        /// </summary>
        public List<IVsSccProject2> GetLoadedControllableProjects()
        {
            var list = new List<IVsSccProject2>();
            // Hashtable mapHierarchies = new Hashtable();

            IVsSolution sol = (IVsSolution)this.GetService(typeof(SVsSolution));
            Guid rguidEnumOnlyThisType = new Guid();
            IEnumHierarchies ppenum = null;
            ErrorHandler.ThrowOnFailure(sol.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref rguidEnumOnlyThisType, out ppenum));

            IVsHierarchy[] rgelt = new IVsHierarchy[1];
            uint pceltFetched = 0;
            while (ppenum.Next(1, rgelt, out pceltFetched) == VSConstants.S_OK &&
                   pceltFetched == 1)
            {
                IVsSccProject2 sccProject2 = rgelt[0] as IVsSccProject2;
                if (sccProject2 != null)
                {
                    list.Add(sccProject2);
                }
            }

            return list;
        }

        public Hashtable GetLoadedControllableProjectsEnum()
        {
            Hashtable mapHierarchies = new Hashtable();

            IVsSolution sol = (IVsSolution)GetService(typeof(SVsSolution));
            Guid rguidEnumOnlyThisType = new Guid();
            IEnumHierarchies ppenum = null;
            ErrorHandler.ThrowOnFailure(sol.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref rguidEnumOnlyThisType, out ppenum));

            IVsHierarchy[] rgelt = new IVsHierarchy[1];
            uint pceltFetched = 0;
            while (ppenum.Next(1, rgelt, out pceltFetched) == VSConstants.S_OK &&
                   pceltFetched == 1)
            {
                IVsSccProject2 sccProject2 = rgelt[0] as IVsSccProject2;
                if (sccProject2 != null)
                {
                    mapHierarchies[rgelt[0]] = true;
                }
            }

            return mapHierarchies;
        }

        /// <summary>
        /// Checks whether a solution exist
        /// </summary>
        /// <returns>True if a solution was created.</returns>
        bool IsThereASolution()
        {
            return (GetSolutionFileName() != null);
        }

        /// <summary>
        /// get the file name of the selected item/document
        /// </summary>
        /// <param name="project"></param>
        /// <param name="item id"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private bool GetItemFileName(IVsProject project, uint itemId, out string filename)
        {
            string bstrMKDocument;

            if (project.GetMkDocument(itemId, out bstrMKDocument) == VSConstants.S_OK
                && !string.IsNullOrEmpty(bstrMKDocument))
            {
                filename = bstrMKDocument;
                return true;
            }

            filename = null;
            return false;
        }

        /// <summary>
        /// find out if the selection list contains the soloution itself
        /// </summary>
        /// <param name="sel"></param>
        /// <returns>isSolutionSelected</returns>
        private bool GetSolutionSelected(IList<VSITEMSELECTION> sel)
        {
            foreach (VSITEMSELECTION vsItemSel in sel)
            {
                if (vsItemSel.pHier == null ||
                    (vsItemSel.pHier as IVsSolution) != null)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Gets the list of selected controllable project hierarchies
        /// </summary>
        /// <returns>True if a solution was created.</returns>
        private Hashtable GetSelectedHierarchies(ref IList<VSITEMSELECTION> sel, out bool solutionSelected)
        {
            // Initialize output arguments
            solutionSelected = false;

            Hashtable mapHierarchies = new Hashtable();
            foreach (VSITEMSELECTION vsItemSel in sel)
            {
                if (vsItemSel.pHier == null ||
                    (vsItemSel.pHier as IVsSolution) != null)
                {
                    solutionSelected = true;
                }

                // See if the selected hierarchy implements the IVsSccProject2 interface
                // Exclude from selection projects like FTP web projects that don't support SCC
                IVsSccProject2 sccProject2 = vsItemSel.pHier as IVsSccProject2;
                if (sccProject2 != null)
                {
                    mapHierarchies[vsItemSel.pHier] = true;
                }
            }

            return mapHierarchies;
        }

        /// <summary>
        /// Gets the list of directly selected VSITEMSELECTION objects
        /// </summary>
        /// <returns>A list of VSITEMSELECTION objects</returns>
        private IList<VSITEMSELECTION> GetSelectedNodes()
        {
            // Retrieve shell interface in order to get current selection
            IVsMonitorSelection monitorSelection = this.GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;
            Debug.Assert(monitorSelection != null, "Could not get the IVsMonitorSelection object from the services exposed by this project");
            if (monitorSelection == null)
            {
                throw new InvalidOperationException();
            }

            List<VSITEMSELECTION> selectedNodes = new List<VSITEMSELECTION>();
            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr selectionContainer = IntPtr.Zero;
            try
            {
                // Get the current project hierarchy, project item, and selection container for the current selection
                // If the selection spans multiple hierachies, hierarchyPtr is Zero
                uint itemid;
                IVsMultiItemSelect multiItemSelect = null;
                ErrorHandler.ThrowOnFailure(monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainer));

                if (itemid != VSConstants.VSITEMID_SELECTION)
                {
                    // We only care if there are nodes selected in the tree
                    if (itemid != VSConstants.VSITEMID_NIL)
                    {
                        if (hierarchyPtr == IntPtr.Zero)
                        {
                            // Solution is selected
                            VSITEMSELECTION vsItemSelection;
                            vsItemSelection.pHier = null;
                            vsItemSelection.itemid = itemid;
                            selectedNodes.Add(vsItemSelection);
                        }
                        else
                        {
                            IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetObjectForIUnknown(hierarchyPtr);
                            // Single item selection
                            VSITEMSELECTION vsItemSelection;
                            vsItemSelection.pHier = hierarchy;
                            vsItemSelection.itemid = itemid;
                            selectedNodes.Add(vsItemSelection);
                        }
                    }
                }
                else
                {
                    if (multiItemSelect != null)
                    {
                        // This is a multiple item selection.

                        //Get number of items selected and also determine if the items are located in more than one hierarchy
                        uint numberOfSelectedItems;
                        int isSingleHierarchyInt;
                        ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectionInfo(out numberOfSelectedItems, out isSingleHierarchyInt));
                        bool isSingleHierarchy = (isSingleHierarchyInt != 0);

                        // Now loop all selected items and add them to the list 
                        Debug.Assert(numberOfSelectedItems > 0, "Bad number of selected itemd");
                        if (numberOfSelectedItems > 0)
                        {
                            VSITEMSELECTION[] vsItemSelections = new VSITEMSELECTION[numberOfSelectedItems];
                            ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectedItems(0, numberOfSelectedItems, vsItemSelections));
                            foreach (VSITEMSELECTION vsItemSelection in vsItemSelections)
                            {
                                selectedNodes.Add(vsItemSelection);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (hierarchyPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPtr);
                }
                if (selectionContainer != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainer);
                }
            }

            return selectedNodes;
        }

        /// <summary>
        /// Returns a list of source controllable files in the selection (recursive)
        /// </summary>
        private IList<string> GetSelectedFilesInControlledProjects()
        {
            IList<VSITEMSELECTION> selectedNodes = null;
            return GetSelectedFilesInControlledProjects(out selectedNodes);
        }

        /// <summary>
        /// Returns a list of source controllable files in the selection
        /// </summary>
        private List<string> GetSelectedFiles(out IList<VSITEMSELECTION> selectedNodes)
        {
            List<string> sccFiles = new List<string>();

            selectedNodes = GetSelectedNodes();

            // now look in the rest of selection and accumulate scc files
            foreach (VSITEMSELECTION vsItemSel in selectedNodes)
            {
                IVsSccProject2 pscp2 = vsItemSel.pHier as IVsSccProject2;
                if (pscp2 == null)
                {
                    // solution case
                    sccFiles.Add(GetSolutionFileName());
                }
                else
                {
                    List<string> nodefilesrec = GetProjectFiles(pscp2, vsItemSel.itemid);
                    foreach (string file in nodefilesrec)
                    {
                        sccFiles.Add(file);
                    }
                }
            }

            return sccFiles;
        }

        /// <summary>
        /// Returns a list of source controllable files in the selection (recursive)
        /// </summary>
        private List<string> GetSelectedFilesInControlledProjects(out IList<VSITEMSELECTION> selectedNodes)
        {
            List<string> sccFiles = new List<string>();

            selectedNodes = GetSelectedNodes();
            bool isSolutionSelected = false;
            Hashtable hash = GetSelectedHierarchies(ref selectedNodes, out isSolutionSelected);
            if (isSolutionSelected)
            {
                // Replace the selection with the root items of all controlled projects
                selectedNodes.Clear();
                Hashtable hashControllable = GetLoadedControllableProjectsEnum();
                foreach (IVsHierarchy pHier in hashControllable.Keys)
                {
                    if (sccService.IsProjectControlled(pHier))
                    {
                        VSITEMSELECTION vsItemSelection;
                        vsItemSelection.pHier = pHier;
                        vsItemSelection.itemid = VSConstants.VSITEMID_ROOT;
                        selectedNodes.Add(vsItemSelection);
                    }
                }

                // Add the solution file to the list
                if (sccService.IsProjectControlled(null))
                {
                    IVsHierarchy solHier = (IVsHierarchy)GetService(typeof(SVsSolution));
                    VSITEMSELECTION vsItemSelection;
                    vsItemSelection.pHier = solHier;
                    vsItemSelection.itemid = VSConstants.VSITEMID_ROOT;
                    selectedNodes.Add(vsItemSelection);
                }
            }

            // now look in the rest of selection and accumulate scc files
            foreach (VSITEMSELECTION vsItemSel in selectedNodes)
            {
                IVsSccProject2 pscp2 = vsItemSel.pHier as IVsSccProject2;
                if (pscp2 == null)
                {
                    // solution case
                    sccFiles.Add(GetSolutionFileName());
                }
                else
                {
                    IList<string> nodefilesrec = GetProjectFiles(pscp2, vsItemSel.itemid);
                    foreach (string file in nodefilesrec)
                    {
                        sccFiles.Add(file);
                    }
                }
            }

            return sccFiles;
        }

        /// <summary>
        /// Returns a list of file names associated with the specified pathStr
        /// </summary>
        static string[] GetFileNamesFromOleBuffer(CALPOLESTR[] pathStr, bool free)
        {
            int nEls = (int)pathStr[0].cElems;
            string[] files = new string[nEls];

            for (int i = 0; i < nEls; i++)
            {
                IntPtr pathIntPtr = Marshal.ReadIntPtr(pathStr[0].pElems, i * IntPtr.Size);
                files[i] = Marshal.PtrToStringUni(pathIntPtr);

                if (free)
                    Marshal.FreeCoTaskMem(pathIntPtr);
            }
            if (free && pathStr[0].pElems != IntPtr.Zero)
                Marshal.FreeCoTaskMem(pathStr[0].pElems);

            return files;
        }

        /// <summary>
        /// Returns a list of source controllable files associated with the specified node
        /// </summary>
        public static IList<string> GetNodeFiles(IVsHierarchy hier, uint itemid)
        {
            IVsSccProject2 pscp2 = hier as IVsSccProject2;
            return GetNodeFiles(pscp2, itemid);
        }

        /// <summary>
        /// Returns a list of source controllable files associated with the specified node
        /// </summary>
        private static IList<string> GetNodeFiles(IVsSccProject2 pscp2, uint itemid)
        {
            // NOTE: the function returns only a list of files, containing both regular files and special files
            // If you want to hide the special files (similar with solution explorer), you may need to return 
            // the special files in a hastable (key=master_file, values=special_file_list)

            // Initialize output parameters
            IList<string> sccFiles = new List<string>();
            if (pscp2 != null)
            {
                CALPOLESTR[] pathStr = new CALPOLESTR[1];
                CADWORD[] flags = new CADWORD[1];

                if (pscp2.GetSccFiles(itemid, pathStr, flags) == 0)
                {
                    //#4  BugFix : Visual Studio Crashing when clicking on Web Reference
                    // The previus MS sample code used 'Marshal.PtrToStringAuto' which caused
                    // a chrash of the studio (2010 only) in some conditions. This also could
                    // be the reason for some further bugs e.g. in commit, update tasks.
                    string[] files = GetFileNamesFromOleBuffer(pathStr, true);
                    for (int elemIndex = 0; elemIndex < pathStr[0].cElems; elemIndex++)
                    {
                        String path = files[elemIndex];
                        sccFiles.Add(path);

                        // See if there are special files
                        if (flags.Length > 0 && flags[0].cElems > 0)
                        {
                            int flag = Marshal.ReadInt32(flags[0].pElems, elemIndex);

                            if (flag != 0)
                            {
                                // We have special files
                                CALPOLESTR[] specialFiles = new CALPOLESTR[1];
                                CADWORD[] specialFlags = new CADWORD[1];

                                pscp2.GetSccSpecialFiles(itemid, path, specialFiles, specialFlags);
                                string[] specialFileNames = GetFileNamesFromOleBuffer(specialFiles, true);
                                foreach (var f in specialFileNames)
                                {
                                    sccFiles.Add(f);
                                }
                            }
                        }
                    }
                }
            }

            return sccFiles;
        }

        /// <summary>
        /// Refreshes the glyphs of the specified hierarchy nodes
        /// </summary>
        public void RefreshNodesGlyphs(IList<VSITEMSELECTION> selectedNodes)
        {
            foreach (VSITEMSELECTION vsItemSel in selectedNodes)
            {
                IVsSccProject2 sccProject2 = vsItemSel.pHier as IVsSccProject2;
                if (vsItemSel.itemid == VSConstants.VSITEMID_ROOT)
                {
                    if (sccProject2 == null)
                    {
                        // Note: The solution's hierarchy does not implement IVsSccProject2, IVsSccProject interfaces
                        // It may be a pain to treat the solution as special case everywhere; a possible workaround is 
                        // to implement a solution-wrapper class, that will implement IVsSccProject2, IVsSccProject and
                        // IVsHierarhcy interfaces, and that could be used in provider's code wherever a solution is needed.
                        // This approach could unify the treatment of solution and projects in the provider's code.

                        // Until then, solution is treated as special case
                        string[] rgpszFullPaths = new string[1];
                        rgpszFullPaths[0] = GetSolutionFileName();
                        VsStateIcon[] rgsiGlyphs = new VsStateIcon[1];
                        uint[] rgdwSccStatus = new uint[1];
                        sccService.GetSccGlyph(1, rgpszFullPaths, rgsiGlyphs, rgdwSccStatus);

                        // Set the solution's glyph directly in the hierarchy
                        IVsHierarchy solHier = (IVsHierarchy)GetService(typeof(SVsSolution));
                        solHier.SetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_StateIconIndex, rgsiGlyphs[0]);
                    }
                    else
                    {
                        // Refresh all the glyphs in the project; the project will call back GetSccGlyphs() 
                        // with the files for each node that will need new glyph
                        sccProject2.SccGlyphChanged(0, null, null, null);
                    }
                }
                else
                {
                    // It may be easier/faster to simply refresh all the nodes in the project, 
                    // and let the project call back on GetSccGlyphs, but just for the sake of the demo, 
                    // let's refresh ourselves only one node at a time
                    IList<string> sccFiles = GetNodeFiles(sccProject2, vsItemSel.itemid);

                    // We'll use for the node glyph just the Master file's status (ignoring special files of the node)
                    if (sccFiles.Count > 0)
                    {
                        string[] rgpszFullPaths = new string[1];
                        rgpszFullPaths[0] = sccFiles[0];
                        VsStateIcon[] rgsiGlyphs = new VsStateIcon[1];
                        uint[] rgdwSccStatus = new uint[1];
                        sccService.GetSccGlyph(1, rgpszFullPaths, rgsiGlyphs, rgdwSccStatus);

                        uint[] rguiAffectedNodes = new uint[1];
                        rguiAffectedNodes[0] = vsItemSel.itemid;
                        sccProject2.SccGlyphChanged(1, rguiAffectedNodes, rgsiGlyphs, rgdwSccStatus);
                    }
                }
            }
        }


        /// <summary>
        /// Returns the filename of the solution
        /// </summary>
        public string GetSolutionFileName()
        {
            IVsSolution sol = (IVsSolution)GetService(typeof(SVsSolution));
            string solutionDirectory, solutionFile, solutionUserOptions;
            if (sol.GetSolutionInfo(out solutionDirectory, out solutionFile, out solutionUserOptions) == VSConstants.S_OK)
            {
                return solutionFile;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the root directory of the solution or first project
        /// </summary>
        public string GetRootDirectory()
        {
            string root = string.Empty;

            var selectedNodes = GetSelectedNodes();
            if (selectedNodes.Count > 0)
            {
                IVsProject pscp = selectedNodes[0].pHier as IVsProject;
                if (pscp != null)
                {
                    String filename;
                    if (GetItemFileName(pscp, selectedNodes[0].itemid, out filename))
                        root = HgLib.Hg.FindRepositoryRoot(filename);
                }
            }

            if (root == string.Empty)
            {
                root = HgLib.Hg.FindRepositoryRoot(GetSolutionFileName());
                if (root == String.Empty)
                {
                    // this is for WebPage projects. the solution file is not included inside the Hg root dir.
                    if (_LastSeenProjectDir != null)
                    {
                        root = HgLib.Hg.FindRepositoryRoot(_LastSeenProjectDir);
                    }
                }
            }

            return root;
        }

        /// <summary>
        /// Returns the filename of the specified controllable project 
        /// </summary>
        public static string GetProjectFileName(VisualHg.SccProvider provider, IVsSccProject2 pscp2Project)
        {
            // Note: Solution folders return currently a name like "NewFolder1{1DBFFC2F-6E27-465A-A16A-1AECEA0B2F7E}.storage"
            // Your provider may consider returning the solution file as the project name for the solution, if it has to persist some properties in the "project file"
            // UNDONE: What to return for web projects? They return a folder name, not a filename! Consider returning a pseudo-project filename instead of folder.

            IVsHierarchy hierProject = (IVsHierarchy)pscp2Project;
            IVsProject project = (IVsProject)pscp2Project;

            // Attempt to get first the filename controlled by the root node 
            IList<string> sccFiles = GetNodeFiles(pscp2Project, VSConstants.VSITEMID_ROOT);
            if (sccFiles.Count > 0 && sccFiles[0] != null && sccFiles[0].Length > 0)
            {
                return sccFiles[0];
            }

            // If that failed, attempt to get a name from the IVsProject interface
            string bstrMKDocument;
            if (project.GetMkDocument(VSConstants.VSITEMID_ROOT, out bstrMKDocument) == VSConstants.S_OK &&
                bstrMKDocument != null && bstrMKDocument.Length > 0)
            {
                return bstrMKDocument;
            }

            // If that failes, attempt to get the filename from the solution
            IVsSolution sol = (IVsSolution)provider.GetService(typeof(SVsSolution));
            string uniqueName;
            if (sol.GetUniqueNameOfProject(hierProject, out uniqueName) == VSConstants.S_OK &&
                uniqueName != null && uniqueName.Length > 0)
            {
                // uniqueName may be a full-path or may be relative to the solution's folder
                if (uniqueName.Length > 2 && uniqueName[1] == ':')
                {
                    return uniqueName;
                }

                // try to get the solution's folder and relativize the project name to it
                string solutionDirectory, solutionFile, solutionUserOptions;
                if (sol.GetSolutionInfo(out solutionDirectory, out solutionFile, out solutionUserOptions) == VSConstants.S_OK)
                {
                    uniqueName = solutionDirectory + "\\" + uniqueName;

                    // UNDONE: eliminate possible "..\\.." from path
                    return uniqueName;
                }
            }

            // If that failed, attempt to get the project name from 
            string bstrName;
            if (hierProject.GetCanonicalName(VSConstants.VSITEMID_ROOT, out bstrName) == VSConstants.S_OK)
            {
                return bstrName;
            }

            // if everything we tried fail, return null string
            return null;
        }

        private static void DebugWalkingNode(IVsHierarchy pHier, uint itemid)
        {
            object property = null;
            if (pHier != null && pHier.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out property) == VSConstants.S_OK)
            {
                Trace.WriteLine(String.Format(CultureInfo.CurrentUICulture, "Walking hierarchy node: {0}", (string)property));
            }
        }

        /// <summary>
        /// Gets the list of ItemIDs that are nodes in the specified project
        /// </summary>
        private static IList<uint> GetProjectItems(IVsHierarchy pHier)
        {
            // Start with the project root and walk all expandable nodes in the project
            return GetProjectItems(pHier, VSConstants.VSITEMID_ROOT);
        }

        /// <summary>
        /// Gets the list of ItemIDs that are nodes in the specified project, starting with the specified item
        /// </summary>
        private static IList<uint> GetProjectItems(IVsHierarchy pHier, uint startItemid)
        {
            List<uint> projectNodes = new List<uint>();

            if (pHier == null)
                return projectNodes;

            // The method does a breadth-first traversal of the project's hierarchy tree
            Queue<uint> nodesToWalk = new Queue<uint>();
            nodesToWalk.Enqueue(startItemid);

            while (nodesToWalk.Count > 0)
            {
                uint node = nodesToWalk.Dequeue();
                projectNodes.Add(node);

                DebugWalkingNode(pHier, node);

                object property = null;
                if (pHier.GetProperty(node, (int)__VSHPROPID.VSHPROPID_FirstChild, out property) == VSConstants.S_OK)
                {
                    uint childnode = (uint)(int)property;
                    if (childnode == VSConstants.VSITEMID_NIL)
                    {
                        continue;
                    }

                    DebugWalkingNode(pHier, childnode);

                    if ((pHier.GetProperty(childnode, (int)__VSHPROPID.VSHPROPID_Expandable, out property) == VSConstants.S_OK && (int)property != 0) ||
                        (pHier.GetProperty(childnode, (int)__VSHPROPID2.VSHPROPID_Container, out property) == VSConstants.S_OK && (bool)property))
                    {
                        nodesToWalk.Enqueue(childnode);
                    }
                    else
                    {
                        projectNodes.Add(childnode);
                    }

                    while (pHier.GetProperty(childnode, (int)__VSHPROPID.VSHPROPID_NextSibling, out property) == VSConstants.S_OK)
                    {
                        childnode = (uint)(int)property;
                        if (childnode == VSConstants.VSITEMID_NIL)
                        {
                            break;
                        }

                        DebugWalkingNode(pHier, childnode);

                        if ((pHier.GetProperty(childnode, (int)__VSHPROPID.VSHPROPID_Expandable, out property) == VSConstants.S_OK && (int)property != 0) ||
                            (pHier.GetProperty(childnode, (int)__VSHPROPID2.VSHPROPID_Container, out property) == VSConstants.S_OK && (bool)property))
                        {
                            nodesToWalk.Enqueue(childnode);
                        }
                        else
                        {
                            projectNodes.Add(childnode);
                        }
                    }
                }

            }

            return projectNodes;
        }

        /// <summary>
        /// Gets the list of source controllable files in the specified project
        /// </summary>
        public static IList<string> GetProjectFiles(IVsSccProject2 pscp2Project)
        {
            return GetProjectFiles(pscp2Project, VSConstants.VSITEMID_ROOT);
        }

        /// <summary>
        /// get current single selected filename
        /// </summary>
        /// <returns></returns>
        public string GetSingleSelectedFileName()
        {
            string filename = string.Empty;
            var selectedNodes = GetSelectedNodes();
            if (selectedNodes.Count == 1)
            {
                IVsProject pscp = selectedNodes[0].pHier as IVsProject;
                if (pscp != null)
                {
                    GetItemFileName(pscp, selectedNodes[0].itemid, out filename);
                }
                else
                {
                    _DTE dte = (_DTE)GetService(typeof(SDTE));

                    if (dte != null && dte.ActiveDocument != null)
                    {
                        filename = dte.ActiveDocument.FullName;
                    }   
                    else
                    {                    
                        filename = GetSolutionFileName();
                    }
                }
            }

            return filename;
        }

        // ------------------------------------------------------------------------
        // find selected file state mask - for quick menu flags detection
        // ------------------------------------------------------------------------
        public bool FindSelectedFirstMask(bool includeChildItems, long stateMask)
        {
            var selectedNodes = GetSelectedNodes();
            foreach (VSITEMSELECTION node in selectedNodes)
            {
                IVsProject pscp = node.pHier as IVsProject;

                string filename = string.Empty;
                if (pscp != null)
                {
                    GetItemFileName(pscp, node.itemid, out filename);
                }
                else
                {
                    filename = GetSolutionFileName();
                }
                if (filename != string.Empty)
                {
                    HgLib.HgFileStatus status = this.sccService.GetFileStatus(filename);
                    if ((stateMask & (long)status) != 0)
                        return true;
                } 
            }

            if (includeChildItems)
            { 
                foreach (VSITEMSELECTION node in selectedNodes)
                {
                    IVsProject pscp = node.pHier as IVsProject; 
                    
                    if (pscp != null)
                    {
                        if(FindProjectSelectedFileStateMask(node.pHier, node.itemid, stateMask))
                           return true;
                    }
                }
            }
            return false;
        }

        // ------------------------------------------------------------------------
        // find selected file state mask 
        // ------------------------------------------------------------------------
        private bool FindProjectSelectedFileStateMask(IVsHierarchy pHier, uint startItemid, long stateMask)
        {
            if (pHier == null)
                return false;
            
            IVsProject pscp = pHier as IVsProject;

            if (pscp == null)
                return false;

            // The method does a breadth-first traversal of the project's hierarchy tree
            Queue<uint> nodesToWalk = new Queue<uint>();
            nodesToWalk.Enqueue(startItemid);

            while (nodesToWalk.Count > 0)
            {
                uint node = nodesToWalk.Dequeue();
                if (CompareFileStateMask(pscp, node, stateMask))
                    return true;

                DebugWalkingNode(pHier, node);

                object property = null;
                if (pHier.GetProperty(node, (int)__VSHPROPID.VSHPROPID_FirstChild, out property) == VSConstants.S_OK)
                {
                    uint childnode = (uint)(int)property;
                    if (childnode == VSConstants.VSITEMID_NIL)
                    {
                        continue;
                    }

                    DebugWalkingNode(pHier, childnode);

                    if ((pHier.GetProperty(childnode, (int)__VSHPROPID.VSHPROPID_Expandable, out property) == VSConstants.S_OK && (int)property != 0) ||
                        (pHier.GetProperty(childnode, (int)__VSHPROPID2.VSHPROPID_Container, out property) == VSConstants.S_OK && (bool)property))
                    {
                        nodesToWalk.Enqueue(childnode);
                    }
                    else
                    {
                        if (CompareFileStateMask(pscp, childnode, stateMask))
                            return true;
                    }

                    while (pHier.GetProperty(childnode, (int)__VSHPROPID.VSHPROPID_NextSibling, out property) == VSConstants.S_OK)
                    {
                        childnode = (uint)(int)property;
                        if (childnode == VSConstants.VSITEMID_NIL)
                        {
                            break;
                        }

                        DebugWalkingNode(pHier, childnode);

                        if ((pHier.GetProperty(childnode, (int)__VSHPROPID.VSHPROPID_Expandable, out property) == VSConstants.S_OK && (int)property != 0) ||
                            (pHier.GetProperty(childnode, (int)__VSHPROPID2.VSHPROPID_Container, out property) == VSConstants.S_OK && (bool)property))
                        {
                            nodesToWalk.Enqueue(childnode);
                        }
                        else
                        {
                            if (CompareFileStateMask(pscp, childnode, stateMask))
                                return true;
                        }
                    }
                }

            }

            return false;
        }

        bool CompareFileStateMask(IVsProject pscp, uint itemid, long stateMask)
        {
          string childFilename = string.Empty;
          if (GetItemFileName(pscp, itemid, out childFilename))
          {
            HgLib.HgFileStatus status = this.sccService.GetFileStatus(childFilename);
            if ((stateMask & (long)status) != 0)
              return true;
          }
          return false;
        }

        /// <summary>
        /// get current selected filenames
        /// </summary>
        /// <returns></returns>
        public List<string> GetSelectedFileNameArray(bool includeChildItems)
        {
            List<string> array = new List<string>();

            var selectedNodes = GetSelectedNodes();
            foreach (VSITEMSELECTION node in selectedNodes)
            {
                IVsProject pscp = node.pHier as IVsProject;

                if (includeChildItems)
                {
                    if (pscp != null)
                    {
                        IList<uint> childItems = GetProjectItems(node.pHier, node.itemid);
                        foreach (uint itemid in childItems)
                        {
                            string childFilename = string.Empty;
                            if (GetItemFileName(pscp, itemid, out childFilename))
                                array.Add(childFilename);
                        }
                    }
                }

                string filename = string.Empty;
                if (pscp != null)
                {
                    if (!includeChildItems)
                        GetItemFileName(pscp, node.itemid, out filename);
                }
                else
                {
                    filename = GetSolutionFileName();
                }
                if (filename != string.Empty)
                    array.Add(filename);
            }

            return array;
        }

        public List<string> GetItemFileNameArray()
        {
            List<string> array = new List<string>();

            var selectedNodes = GetSelectedNodes();
            foreach (VSITEMSELECTION node in selectedNodes)
            {
                string filename = string.Empty;
                IVsProject pscp = node.pHier as IVsProject;
                if (pscp != null)
                {
                    GetItemFileName(pscp, node.itemid, out filename);
                }
                else
                {
                    filename = GetSolutionFileName();
                }
                array.Add(filename);
            }

            return array;
        }

        /// <summary>
        /// Gets the list of source controllable files in the specified project
        /// </summary>
        public static List<string> GetProjectFiles(IVsSccProject2 pscp2Project, uint startItemId)
        {
            List<string> projectFiles = new List<string>();
            IVsHierarchy hierProject = (IVsHierarchy)pscp2Project;
            IList<uint> projectItems = GetProjectItems(hierProject, startItemId);

            foreach (uint itemid in projectItems)
            {
                IList<string> sccFiles = GetNodeFiles(pscp2Project, itemid);
                foreach (string file in sccFiles)
                {
                    projectFiles.Add(file);
                }
            }

            return projectFiles;
        }

        /// <summary>
        /// Checks whether the provider is invoked in command line mode
        /// </summary>
        public bool InCommandLineMode()
        {
            IVsShell shell = (IVsShell)GetService(typeof(SVsShell));
            object pvar;
            if (shell.GetProperty((int)__VSSPROPID.VSSPROPID_IsInCommandLineMode, out pvar) == VSConstants.S_OK &&
                (bool)pvar)
            {
                return true;
            }

            return false;
        }

        #endregion

        [DllImport("user32.dll")]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);
        
        /// <summary>
        /// set current branch in application window title
        /// </summary>
        /// <returns></returns>
        public void UpdateMainWindowTitle(string branch)
        {
            _DTE dte = (_DTE)GetService(typeof(SDTE));

            if (dte != null && dte.MainWindow  != null)
            {
                string caption = dte.MainWindow.Caption;

                // strip prev branch name
                string[]  param = caption.Split('-');
                if (param.Length > 1)
                {
                    int index = param[0].IndexOf('(');
                    if (index > 0)
                        param[0] = param[0].Substring(0, index);

                    // add new branch name
                    string newCaption = string.Empty;
                    
                    foreach (string s in param)
                    {
                        if (newCaption == string.Empty && branch != string.Empty)
                            newCaption = s + "(" + branch + ") ";
                        else
                            newCaption += s;

                    }

                    if (caption != newCaption)
                    {
                        IntPtr hWnd = (IntPtr)dte.MainWindow.HWnd;
                        SetWindowText(hWnd, newCaption);
                    }
                }
            }
        }
    }
}
