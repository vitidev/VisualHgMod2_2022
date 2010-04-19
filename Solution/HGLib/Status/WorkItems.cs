using System;
using System.Collections.Generic;
using System.Text;

namespace HGLib
{
    // ---------------------------------------
    // work item for user and IDE commands 
    // ---------------------------------------
    public interface IHGWorkItem
    {
        void Do(HGStatus status, List<string> ditryFilesList);
    }

    // ------------------------------------------------------------------------
    /// track file renamed event
    // ------------------------------------------------------------------------
    public class TrackFilesRenamed : IHGWorkItem
    {
        string[] filenameO = null;
        string[] filenameN = null;

        public TrackFilesRenamed(string[] O, string[] N)
        {
            filenameO = O;
            filenameN = N;
        }

        public virtual void Do(HGStatus status, List<string> ditryFilesList)
        {
            status.EnterFileRenamed(filenameO, filenameN);
            ditryFilesList.AddRange(filenameN);
        }
    }

    // ------------------------------------------------------------------------
    /// track file removed event
    // ------------------------------------------------------------------------
    public class TrackFileRemoved : IHGWorkItem
    {
        string[] filenames = null;
        
        public TrackFileRemoved(string[] R)
        {
            filenames = R;
        }

        public virtual void Do(HGStatus status, List<string> ditryFilesList)
        {
            status.EnterFilesRemoved(filenames);
            ditryFilesList.AddRange(filenames);
        }
    }

    // ------------------------------------------------------------------------
    /// track files added event
    // ------------------------------------------------------------------------
    public class TrackFilesAddedNotIgnored : IHGWorkItem
    {
        string[] fileList = null;

        public TrackFilesAddedNotIgnored(string[] N)
        {
            fileList = N;
        }
        public virtual void Do(HGStatus status, List<string> ditryFilesList)
        {
            status.AddNotIgnoredFiles(fileList);
        }
    }

    // ------------------------------------------------------------------------
    // adds the given project/directory if not exsist. The status of the files
    // contining in the directory will be scanned by a QueryRootStatus call.
    // ------------------------------------------------------------------------
    public class UpdateRootDirectoryAdded : IHGWorkItem
    {
        string directory = string.Empty;
        
        public UpdateRootDirectoryAdded(string directory)
        {
            this.directory = directory;
        }

        public virtual void Do(HGStatus status, List<string> ditryFilesList)
        {
            status.AddRootDirectory(directory);
        }
    }

    // ---------------------------------------
    // queued user commands or events from the IDE
    // ---------------------------------------
    class WorkItemQueue : Queue<IHGWorkItem>
    {
        // thread save copy and clear of this queue
        public WorkItemQueue PopWorkItems()
        {
            WorkItemQueue q = new WorkItemQueue();
            lock (this)
            {
                foreach (IHGWorkItem item in this)
                {
                    q.Enqueue(item);
                }
                Clear();
            }
            
            return q;
        }
    }
}
