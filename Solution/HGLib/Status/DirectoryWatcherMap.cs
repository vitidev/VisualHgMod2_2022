using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HgLib
{
    public class DirectoryWatcherMap
    {
        List<DirectoryWatcher> _watchers;
        object _syncRoot;


        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _watchers.Count;
                }
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                _watchers.Clear();
            }
        }

        public DirectoryWatcher[] Watchers
        {
            get
            {
                lock (_syncRoot)
                {
                    return _watchers.ToArray();
                }
            }
        }


        public DirectoryWatcherMap()
        {
            _watchers = new List<DirectoryWatcher>();
            _syncRoot = new object();
        }


        public void EnableDirectoryWatching(bool enable)
        {
            lock (_syncRoot)
            {
                foreach (var watcher in _watchers)
                {
                    watcher.EnableDirectoryWatching(enable);
                }
            }
        }

        public bool ContainsDirectory(string directory)
        {
            lock (_syncRoot)
            {
                return _watchers.Any(x => x.Directory.Equals(directory, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public void WatchDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            lock (_syncRoot)
            {
                if (ContainsDirectory(directory))
                {
                    return;
                }

                var addNewWatcher = true;
                var removeWatcher = new List<DirectoryWatcher>();
                var directorySlash = directory + "\\";

                foreach (var watcher in _watchers)
                {
                    var watcherDirectorySlash = watcher.Directory + "\\";

                    if (watcherDirectorySlash.IndexOf(directorySlash) == 0)
                    {
                        removeWatcher.Add(watcher); // sub-directory of new watcher
                    }
                    else if (directorySlash.IndexOf(watcherDirectorySlash) == 0)
                    { 
                        addNewWatcher = false; // directory already watched
                    }
                }

                if (addNewWatcher)
                {
                    for (int pos = 0; pos < removeWatcher.Count; ++pos)
                    {
                        var watcher = removeWatcher[pos];
                        _watchers.Remove(watcher);
                        watcher.EnableDirectoryWatching(false);
                    }

                    _watchers.Add(new DirectoryWatcher(directory));
                }
            }
        }


        public DateTime GetLatestChange()
        {
            lock (_syncRoot)
            {
                var latestChange = _watchers.Count > 0 ? DateTime.Today : DateTime.Now;
                
                foreach (var watcher in _watchers)
                {
                    if (watcher.LastChange > latestChange)
                    {
                        latestChange = watcher.LastChange;
                    }
                }

                return latestChange;
            }
        }

        public long GetNumberOfChangedFiles()
        {
            lock (_syncRoot)
            {
                long count = 0;

                foreach (var watcher in _watchers)
                {
                    count += watcher.DirtyFilesCount;
                }

                return count;
            }
        }

        public void UnsubscribeEvents()
        {
            lock (_syncRoot)
            {
                foreach (var watcher in _watchers)
                {
                    watcher.UnsubscribeEvents();
                }
            }
        }
    }
}
