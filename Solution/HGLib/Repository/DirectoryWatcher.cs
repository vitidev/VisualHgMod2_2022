using System;
using System.Collections.Generic;
using System.IO;

namespace HgLib.Repository
{
    internal class DirectoryWatcher : IDisposable
    {
        private List<string> _dirtyFiles;
        private FileSystemWatcher _watcher;

        public object SyncRoot { get; private set; }

        public string Directory { get; private set; }

        public DateTime LastChange { get; private set; }

        public int DirtyFilesCount
        {
            get
            {
                lock (SyncRoot)
                {
                    return _dirtyFiles.Count;
                }
            }
        }

        public bool FileSystemWatch
        {
            set
            {
                lock (SyncRoot)
                {
                    _watcher.EnableRaisingEvents = value;
                }
            }
        }


        public DirectoryWatcher(string directory, object syncRoot)
        {
            SyncRoot = syncRoot;
            Directory = directory;

            _dirtyFiles = new List<string>();

            _watcher = new FileSystemWatcher
            {
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

        public void UnsubscribeEvents()
        {
            lock (SyncRoot)
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
            lock (SyncRoot)
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
            lock (SyncRoot)
            {
                AddDirtyFile(e.OldFullPath);
                AddDirtyFile(e.FullPath);
            }
        }

        private void AddDirtyFile(string path)
        {
            lock (SyncRoot)
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
