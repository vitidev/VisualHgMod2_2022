using System;
using System.Collections.Generic;
using System.IO;

namespace HgLib
{
    public class DirectoryWatcher : IDisposable
    {
        FileSystemWatcher _watcher;
        List<string> _dirtyFiles;
        DateTime _lastChange = DateTime.Today;
        object _syncRoot;

        public string Directory { get; private set; }

        public DateTime LastChange
        {
            get { return _lastChange; }
            set { _lastChange = value; }
        }

        public int DirtyFilesCount
        {
            get
            {
                lock (_syncRoot)
                {
                    return _dirtyFiles.Count;
                }
            }
        }


        public DirectoryWatcher(string directory)
        {
            Directory = directory;

            _dirtyFiles = new List<string>();
            _syncRoot = new object();

            _watcher = new FileSystemWatcher {
                Path = directory,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName |
                    NotifyFilters.Attributes |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Size |
                    NotifyFilters.CreationTime |
                    NotifyFilters.DirectoryName,
            };

            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
            _watcher.Deleted += OnChanged;
            _watcher.Renamed += OnRenamed;

            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }

        public void EnableDirectoryWatching(bool enable)
        {
            lock (_syncRoot)
            {
                _watcher.EnableRaisingEvents = enable;
            }
        }

        public void UnsubscribeEvents()
        {
            lock (_syncRoot)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnChanged;
                _watcher.Created -= OnChanged;
                _watcher.Deleted -= OnChanged;
                _watcher.Renamed -= OnRenamed;
            }
        }

        public string[] DumpDirtyFiles()
        {
            lock (_syncRoot)
            {
                var dump = _dirtyFiles.ToArray();
                
                _dirtyFiles.Clear();

                return dump;
            }
        }

        
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (IsVisualStudioTempFile(e.FullPath))
            {
                return;
            }

            AddDirtyFile(e.FullPath);
        }

        private static bool IsVisualStudioTempFile(string path)
        {
            return path.EndsWith(".tmp", StringComparison.InvariantCultureIgnoreCase) && 
                (path.IndexOf("~RF") > -1 || path.IndexOf("\\ve-") > -1);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            lock (_syncRoot)
            {
                AddDirtyFile(e.OldFullPath);
                AddDirtyFile(e.FullPath);
            }
        }

        private void AddDirtyFile(string path)
        {
            lock (_syncRoot)
            {
                if (!_dirtyFiles.Contains(path))
                {
                    _dirtyFiles.Add(path);
                }

                LastChange = DateTime.Now;
            }
        }
    }
}
