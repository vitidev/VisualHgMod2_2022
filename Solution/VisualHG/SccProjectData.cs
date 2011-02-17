using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Diagnostics;


namespace VisualHG
{
    /// <summary>
    /// Enum of project types with workarounds
    /// </summary>
    enum SccProjectType
    {
        Normal,
        SolutionFolder,
        WebSite,
    }

    [DebuggerDisplay("Project={ProjectName}, ProjectType={_projectType}")]
    class SccProjectData
    {
        string _projectName;
        string _projectDirectory;

        readonly IVsSccProject2 _sccProject;
        readonly IVsHierarchy   _hierarchy;
        readonly IVsProject     _vsProject;
        readonly SccProjectType _projectType;
        //readonly SccProvider    _context;

        public SccProjectData(IVsSccProject2 project)
        {
            /*if (context == null)
                throw new ArgumentNullException("context");
            else if (project == null)
                throw new ArgumentNullException("project");

            _context = context;
            */
            // Project references to speed up marshalling
            _sccProject = project;
            _hierarchy = (IVsHierarchy)project; // A project must be a hierarchy in VS
            _vsProject = (IVsProject)project; // A project must be a VS project

            _projectType = GetProjectType(project);

            _projectName = ProjectName((IVsHierarchy)project);
            _projectDirectory = ProjectDirectory((IVsHierarchy)project); ;
        }

        public bool IsSolutionFolder
        {
            get { return _projectType == SccProjectType.SolutionFolder; }
        }

        public bool IsWebSite
        {
            get { return _projectType == SccProjectType.WebSite; }
        }

        static public string ProjectName(IVsHierarchy hierarchy)
        {
            string projectName = "";
            if (hierarchy != null)
            {
                object name;

                if (ErrorHandler.Succeeded(hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Name, out name)))
                {
                    projectName = name as string;
                }
            }

            return projectName;
        }

        /// <summary>
        /// Gets the project directory.
        /// </summary>
        /// <value>The project directory or null if the project does not have one</value>
        static public string ProjectDirectory(IVsHierarchy hierarchy)
        {
            string projectDirectory = "";
            if (hierarchy != null)
            {
                projectDirectory = "";
                object name;

                if (ErrorHandler.Succeeded(hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir, out name)))
                {
                    string dir = name as string;

                    if (dir != null)
                        dir = GetNormalizedFullPath(dir);

                    projectDirectory = dir;
                }
            }
            return projectDirectory;
        }

        /// <summary>
        /// Checks whether the specified project is a solution folder
        /// </summary>
        private static readonly Guid _solutionFolderProjectId = new Guid("2150e333-8fdc-42a3-9474-1a3956d46de8");
        private static readonly Guid _websiteProjectId = new Guid("e24c65dc-7377-472b-9aba-bc803b73c61a");
        static SccProjectType GetProjectType(IVsSccProject2 project)
        {
            IPersistFileFormat pFileFormat = project as IPersistFileFormat;
            if (pFileFormat != null)
            {
                Guid guidClassID;
                if (VSConstants.S_OK != pFileFormat.GetClassID(out guidClassID))
                    return SccProjectType.Normal;

                if (guidClassID == _solutionFolderProjectId)
                    return SccProjectType.SolutionFolder;
                else if (guidClassID == _websiteProjectId)
                    return SccProjectType.WebSite;
            }

            return SccProjectType.Normal;
        }

        static String GetNormalizedFullPath(String path)
        {
	        if (String.IsNullOrEmpty(path))
		        throw new ArgumentNullException("path");

	        path = Path.GetFullPath(path);

	        if(path.Length >= 2 && path[1] == ':')
	        {
		        char c = path[0];

                if ((c >= 'a') && (c <= 'z'))
                {
                    path = c.ToString().ToUpperInvariant() + path.Substring(1);
                }

		        String r = path.TrimEnd('\\');

		        if(r.Length > 3)
			        return r;
		        else
			        return path.Substring(0, 3);
	        }
	        else if(path.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
	        {
		        String root = Path.GetPathRoot(path).ToLowerInvariant();
        	
		        if(!path.StartsWith(root, StringComparison.Ordinal))
			        path = root + path.Substring(root.Length).TrimEnd('\\');
	        }
	        else
		        path = path.TrimEnd('\\');

	        return path;
        }
    }
}
