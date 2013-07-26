using System;
using System.Collections.Generic;
using System.Text;

namespace HgLib
{
    // ---------------------------------------
    // work item for user and IDE commands 
    // ---------------------------------------
    public interface IHgWorkItem
    {
        void Do(HgStatus status, List<string> ditryFilesList);
    }

    // ------------------------------------------------------------------------
    /// track file renamed event
    // ------------------------------------------------------------------------
    public class TrackFilesRenamed : IHgWorkItem
    {
        string[] filenameO = null;
        string[] filenameN = null;

        public TrackFilesRenamed(string[] O, string[] N)
        {
            filenameO = O;
            filenameN = N;
        }

        public virtual void Do(HgStatus status, List<string> ditryFilesList)
        {
            status.EnterFileRenamed(filenameO, filenameN);
            ditryFilesList.AddRange(filenameN);
        }
    }

    // ------------------------------------------------------------------------
    /// track file removed event
    // ------------------------------------------------------------------------
    public class TrackFileRemoved : IHgWorkItem
    {
        string[] filenames = null;
        
        public TrackFileRemoved(string[] R)
        {
            filenames = R;
        }

        public virtual void Do(HgStatus status, List<string> ditryFilesList)
        {
            status.EnterFilesRemoved(filenames);
            ditryFilesList.AddRange(filenames);
        }
    }

    // ------------------------------------------------------------------------
    /// track files added event
    // ------------------------------------------------------------------------
    public class TrackFilesAddedNotIgnored : IHgWorkItem
    {
        string[] fileList = null;

        public TrackFilesAddedNotIgnored(string[] N)
        {
            fileList = N;
        }
        public virtual void Do(HgStatus status, List<string> ditryFilesList)
        {
            // check also for new sub root dir
            foreach(string file in fileList)
                status.AddRootDirectory(file); 
            
            // add files
            status.AddNotIgnoredFiles(fileList);
        }
    }

    // ------------------------------------------------------------------------
    // adds the given project/directory if not exsist. The status of the files
    // contining in the directory will be scanned by a QueryRootStatus call.
    // ------------------------------------------------------------------------
    public class UpdateRootDirectoryAdded : IHgWorkItem
    {
        string directory = string.Empty;
        
        public UpdateRootDirectoryAdded(string directory)
        {
            this.directory = directory;
        }

        public virtual void Do(HgStatus status, List<string> ditryFilesList)
        {
            status.AddRootDirectory(directory);
        }
    }

    // ------------------------------------------------------------------------
    // triggers an status update for the given file
    // ------------------------------------------------------------------------
    public class UpdateFileStatusCommand : IHgWorkItem
    {
        string[] file = null;

        public UpdateFileStatusCommand(string[] file)
        {
            this.file = file;
        }

        public virtual void Do(HgStatus status, List<string> ditryFilesList)
        {
            status.UpdateFileStatus(file);
        }
    }

    // ------------------------------------------------------------------------
    // triggers an status update for the given file
    // ------------------------------------------------------------------------
    public class UpdateRootStatusCommand : IHgWorkItem
    {
        string root;
        public UpdateRootStatusCommand(string root)
        {
            this.root = root;
        }

        public virtual void Do(HgStatus status, List<string> ditryFilesList)
        {
            status.UpdateFileStatus(root);
        }
    }

    // ---------------------------------------
    // queued user commands or events from the IDE
    // ---------------------------------------
    class WorkItemQueue : Queue<IHgWorkItem>
    {
        // thread save copy and clear of this queue
        public WorkItemQueue PopWorkItems()
        {
            WorkItemQueue q = new WorkItemQueue();
            lock (this)
            {
                foreach (IHgWorkItem item in this)
                {
                    q.Enqueue(item);
                }
                Clear();
            }
            
            return q;
        }
    }
}
