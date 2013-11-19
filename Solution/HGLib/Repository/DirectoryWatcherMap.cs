using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HgLib
{
    internal class DirectoryWatcherMap : IDisposable
    {
        private List<DirectoryWatcher> _watchers;
        
        public object SyncRoot { get; private set; }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return _watchers.Count;
                }
            }
        }
        
        public int DirtyFilesCount
        {
            get
            {
                lock (SyncRoot)
                {
                    var count = 0;

                    foreach (var watcher in _watchers)
                    {
                        count += watcher.DirtyFilesCount;
                    }

                    return count;
                }
            }
        }
        
        public DateTime LatestChange
        {
            get
            {
                lock (SyncRoot)
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
        }

        public bool FileSystemWatch
        {
            set
            {
                lock (SyncRoot)
                {
                    foreach (var watcher in _watchers)
                    {
                        watcher.FileSystemWatch = value;
                    }
                }
            }
        }


        public DirectoryWatcherMap()
        {
            _watchers = new List<DirectoryWatcher>();
            SyncRoot = new object();
        }


        public void Dispose()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }
        }


        public bool ContainsDirectory(string directory)
        {
            lock (SyncRoot)
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

            lock (SyncRoot)
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
                        watcher.FileSystemWatch = false;
                    }

                    _watchers.Add(new DirectoryWatcher(directory, SyncRoot));
                }
            }
        }


        public void Clear()
        {
            lock (SyncRoot)
            {
                UnsubscribeEvents();
                _watchers.Clear();
            }
        }
        
        private void UnsubscribeEvents()
        {
            foreach (var watcher in _watchers)
            {
                watcher.UnsubscribeEvents();
            }
        }


        public string[] DumpDirtyFiles()
        {
            lock (SyncRoot)
            {
                return _watchers.SelectMany(x => x.DumpDirtyFiles()).ToArray(); // NOTE: DumpDirtyFiles has side effects
            }
        }
    }
}