using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HGLib
{
    // ------------------------------------------------------------------------
    // archive directories watcher map - threadsafe implementation 
    // ------------------------------------------------------------------------
    public class DirectoryWatcherMap
    {
        // this map contains the directory watcher objects
        // one for each hg root directory
        Dictionary<string, DirectoryWatcher> dict = new Dictionary<string, DirectoryWatcher>();

        // ------------------------------------------------------------------------
        // get number of watcher
        // ------------------------------------------------------------------------
        public int Count
        {
            get { lock (dict) { return dict.Count; } }
        }

        // ------------------------------------------------------------------------
        // clear watcher
        // ------------------------------------------------------------------------
        public void Clear()
        {
            lock (dict) { dict.Clear(); }
        }

        // ------------------------------------------------------------------------
        // threadsafe access to the watcher map
        // ------------------------------------------------------------------------
        public Dictionary<string, DirectoryWatcher> WatcherList
        {
            get
            {
                lock (dict)
                {
                    return new Dictionary<string, DirectoryWatcher>(dict);
                }
            }
        }

        // ------------------------------------------------------------------------
        // toggle directory watching on / off
        // ------------------------------------------------------------------------
        public void EnableDirectoryWatching(bool enable)
        {
            lock (dict)
            {
                foreach (var kvp in dict)
                {
                    kvp.Value.EnableDirectoryWatching(enable);
                }
            }
        }

        // ------------------------------------------------------------------------
        // check existance of an directory
        // ------------------------------------------------------------------------
        public bool ContainsDirectory(string directory)
        {
            lock (dict)
            {
                bool dirExists = false;
                DirectoryWatcher value;
                dirExists = dict.TryGetValue(directory.ToLower(), out value);
                return dirExists;
            }
        }

        // ------------------------------------------------------------------------
        /// update watcher objects for the given directory 
        // ------------------------------------------------------------------------
        public bool WatchDirectory(string directory)
        {
            bool retval = DirectoryWatcher.DirectoryExists(directory);
            if (retval)
            {
                lock (dict)
                {
                    string key = directory.ToLower();
                    DirectoryWatcher value;
                    if (!dict.TryGetValue(key, out value))
                    {
                        bool addNewWatcher = true;
                        List<DirectoryWatcher> removeWatcher = new List<DirectoryWatcher>();
                        string directorySlash = directory + "\\";
                        foreach (DirectoryWatcher watcher in dict.Values)
                        {
                            string watcherDirectorySlash = watcher._directory + "\\";
                            if (watcherDirectorySlash.IndexOf(directorySlash) == 0)
                                removeWatcher.Add(watcher); // sub-directory of new watcher
                            else if (directorySlash.IndexOf(watcherDirectorySlash) == 0)
                                addNewWatcher = false; // directory already watched
                        }

                        if (addNewWatcher)
                        {
                            // remove no longer used watcher objects
                            for (int pos = 0; pos < removeWatcher.Count; ++pos)
                            {
                                DirectoryWatcher watcher = removeWatcher[pos];
                                dict.Remove(watcher._directory);
                                watcher.EnableDirectoryWatching(false);
                                watcher = null;
                            }

                            dict[directory] = new DirectoryWatcher(directory);
                        }
                    }
                }
            }
            return retval;
        }

        // ------------------------------------------------------------------------
        // get the time stamp in ms of the latest change event
        // ------------------------------------------------------------------------
        public DateTime GetLatestChange()
        {
            lock (dict)
            {
                DateTime latestTime = dict.Count > 0 ? DateTime.Today : DateTime.Now;
                foreach (var kvp in dict)
                {
                    DateTime stamp = kvp.Value.LastChangeEvent;
                    if (stamp > latestTime)
                        latestTime = stamp;
                }
                return latestTime;
            }
        }

        // ------------------------------------------------------------------------
        // get the total count of changed files
        // ------------------------------------------------------------------------
        public long GetNumberOfChangedFiles()
        {
            lock (dict)
            {
                long retval = 0;
                foreach (var kvp in dict)
                {
                    retval += kvp.Value.DirtyFilesCount;
                }
                return retval;
            }
        }

        public void UnsubscribeEvents()
        {
            lock (dict)
            {
                foreach (var kvp in dict)
                {
                    kvp.Value.UnsubscribeEvents();
                }
            }
        }
    }
}
