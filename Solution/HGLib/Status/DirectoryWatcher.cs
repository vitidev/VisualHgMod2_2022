using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace HGLib
{
    // ------------------------------------------------------------------------
    // system directory watcher
    // ------------------------------------------------------------------------
    public class DirectoryWatcher
    {
        // file watcher object
        FileSystemWatcher _watcher = new FileSystemWatcher();

        // dirty files map - the bool value is unused
        Dictionary<string, bool> _dirtyFilesMap = new Dictionary<string, bool>();
        // the wached directory
        public string _directory;
        // last seen file change event in nano sec elapsed since 12:00:00 midnight, January 1, 0001
        DateTime _lastChangeEvent = DateTime.Today;
        
        public DateTime LastChangeEvent
        {
            get{ return _lastChangeEvent; }
            set{ _lastChangeEvent = value; }
        }

        // ------------------------------------------------------------------------
        // connect watcher event handler and fill file filter lists
        // ------------------------------------------------------------------------
        public DirectoryWatcher(string directory)
        {
            _directory = directory;

            _watcher.Path = directory;
            _watcher.IncludeSubdirectories = true;
            //_fileSystemWatcher.Filter = m_filter.Text;
            _watcher.NotifyFilter =
                            NotifyFilters.FileName
                            | NotifyFilters.Attributes
                //| NotifyFilters.LastAccess 
                            | NotifyFilters.LastWrite
                //| NotifyFilters.Security 
                            | NotifyFilters.Size
                            | NotifyFilters.CreationTime
                            | NotifyFilters.DirectoryName;


            // Add file changed event handler
            _watcher.Changed += new FileSystemEventHandler(OnChanged);
            _watcher.Created += new FileSystemEventHandler(OnChanged);
            _watcher.Deleted += new FileSystemEventHandler(OnChanged);
            _watcher.Renamed += new RenamedEventHandler(OnRenamedEvent);

            // Begin watching
            _watcher.EnableRaisingEvents = true;
        }

        // ------------------------------------------------------------------------
        // enable / disable raising event
        // ------------------------------------------------------------------------
        public void EnableRaisingEvents(bool enable)
        {
            lock (_watcher)
            {
                _watcher.EnableRaisingEvents = enable;
            }
        }

        // get the number of dirty files
        public int DirtyFilesCount
        {
            get { lock (_dirtyFilesMap)
                { return _dirtyFilesMap.Count; }}
        }

        // replaces the current dirty files map with a new one.
        // and then returns the 'old' map.
        public Dictionary<string, bool> PopDirtyFilesMap()
        {
            Dictionary<string, bool> retval = null; 
            lock (_dirtyFilesMap)
            {
                retval = _dirtyFilesMap;
                _dirtyFilesMap = new Dictionary<string, bool>();
            }
            return retval;
        }

        // ------------------------------------------------------------------------
        // unhook watcher events and disable raising event 
        // ------------------------------------------------------------------------
        public void UnsubscribeEvents()
        {
            lock (_watcher)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= new FileSystemEventHandler(OnChanged);
                _watcher.Created -= new FileSystemEventHandler(OnChanged);
                _watcher.Deleted -= new FileSystemEventHandler(OnChanged);
                _watcher.Renamed -= new RenamedEventHandler(OnRenamedEvent);
            }
        }

        // ------------------------------------------------------------------------
        // filter the changed file name
        // ------------------------------------------------------------------------
        static bool Filter(string fullPath, Dictionary<string, bool> extensionWhiteList)
        {
            int index = fullPath.LastIndexOf('\\');
            if (index != -1 )
            {
                string fileName = fullPath.Substring(index + 1).ToLower();

                index = fileName.LastIndexOf('.');
                if (index > 0)
                {
                    if (fileName.Contains("csproj.filelistabsolute.txt"))
                    {
                        return false;
                    }

                    string extension = fileName.Substring(index + 1).ToLower();
                    if (extensionWhiteList.ContainsKey(extension))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // ------------------------------------------------------------------------
        // file changed event received from directory watcher object
        // ------------------------------------------------------------------------
        void OnChanged(object source, FileSystemEventArgs fsea)
        {
            lock (_dirtyFilesMap)
            {
                _dirtyFilesMap[fsea.FullPath] = true;
            }
            LastChangeEvent = DateTime.Now;
        }

        // ------------------------------------------------------------------------
        // file renamed event received from directory watcher object
        // ------------------------------------------------------------------------
        void OnRenamedEvent(Object source, RenamedEventArgs rea)
        {
            lock (_dirtyFilesMap)
            {
                _dirtyFilesMap[rea.OldFullPath] = true;
                _dirtyFilesMap[rea.FullPath] = true;
            }
            LastChangeEvent = DateTime.Now;
        }
    }
}
