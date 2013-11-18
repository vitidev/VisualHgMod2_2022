using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace HgLib
{
    public class HgRepository
    {
        public event EventHandler StatusChanged = (s, e) => { };
        
        private const int UpdateInterval = 2000;

        private int _updatingLevel;
        private HgCommandQueue _commands;
        private DirectoryWatcherMap _directoryWatchers;
        private HgFileInfoDictionary _cache;
        private Dictionary<string, string> _roots;

        private System.Timers.Timer _updateTimer;

        private volatile bool _cacheUpdateRequired;


        public bool IsEmpty
        {
            get { return _directoryWatchers.Count == 0; }
        }
        
        public bool CacheUpdateRequired
        {
            set { _cacheUpdateRequired = value; }
        }

        public bool FileSystemWatch
        {
            set { _directoryWatchers.FileSystemWatch = value; }
        }


        public HgRepository()
        {
            Initialize();

            _updateTimer.Start();
        }

        private void Initialize()
        {
            _cache = new HgFileInfoDictionary();
            _directoryWatchers = new DirectoryWatcherMap();
            _commands = new HgCommandQueue();
            _roots = new Dictionary<string, string>();
            
            _updateTimer = new System.Timers.Timer
            { 
                AutoReset = false,
                Interval = 100,
            };

            _updateTimer.Elapsed += OnTimerElapsed;
        }


        public void Enqueue(HgCommand command)
        {
            lock (_commands)
            {
                _commands.Enqueue(command);
            }
        }


        public void AddFiles(string[] fileNames)
        {
            try
            {
                BeginUpdate();
                Cache(Hg.AddFiles(fileNames, HgFileStatus.NotTracked));
            }
            finally
            {
                EndUpdate();
            }
        }
        
        public void RemoveFiles(string[] fileNames)
        {
            try
            {
                BeginUpdate();
                Cache(Hg.RemoveFiles(fileNames));
            }
            finally
            {
                EndUpdate();
            }
        }

        public void RenameFiles(string[] fileNames, string[] newFileNames)
        {
            lock (_cache.SyncRoot)
            {
                foreach (var fileName in fileNames.Concat(newFileNames))
                {
                    _cache.Remove(fileName);
                }
            }

            try
            {
                BeginUpdate();
                Cache(Hg.RenameFiles(fileNames, newFileNames));
            }
            catch
            {
                EndUpdate();
            }
        }

        public void UpdateFileStatus(string[] fileNames)
        {
            Cache(Hg.GetFileInfo(fileNames));
        }

        public void UpdateRootStatus(string path)
        {
            var root = HgPath.FindRepositoryRoot(path);

            if (String.IsNullOrEmpty(root))
            {
                return;
            }

            _directoryWatchers.WatchDirectory(root);
            _roots[root] = Hg.GetCurrentBranchName(root);

            Cache(Hg.GetRootStatus(root));
        }


        public string GetBranchNames()
        {
            lock (_roots)
            {
                return _roots.Count > 0 ? _roots.Values.Distinct().Aggregate((x, y) => String.Concat(x, ", ", y)) : "";
            }
        }

        public string GetDirectoryBranch(string directory)
        {
            var branch = "";

            _roots.TryGetValue(directory, out branch);
            
            return branch;
        }

        public HgFileStatus GetFileStatus(string fileName)
        {
            var fileInfo = _cache[fileName];

            return fileInfo != null ? fileInfo.Status : HgFileStatus.NotTracked;
        }

        public HgFileInfo[] GetPendingFiles()
        {
            return _cache.GetPendingFiles();
        }


        public void ClearCache()
        {
            lock (_directoryWatchers.SyncRoot)
            {
                _directoryWatchers.UnsubscribeEvents();
                _directoryWatchers.Clear();
            }

            lock (_roots)
            {
                _roots.Clear();
            }

            _cache.Clear();
        }

        
        private void BeginUpdate()
        {
            _updatingLevel++;
        }

        private void EndUpdate()
        {
            _updatingLevel = Math.Max(0, _updatingLevel - 1);
        }


        private void Cache(HgFileInfo[] files)
        {
            _cache.Add(files);
        }

        private void UpdateCache()
        {
            try
            {
                BeginUpdate();

                _cacheUpdateRequired = false;
                
                _cache.Clear();
                _directoryWatchers.DumpDirtyFiles();

                foreach (var root in GetRoots().Where(x => !String.IsNullOrEmpty(x)))
                {
                    Cache(Hg.GetRootStatus(root));
                    _roots[root] = Hg.GetCurrentBranchName(root);
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        private List<string> GetRoots()
        {
            List<string> roots = null;
            
            lock (_roots)
            {
                roots = new List<string>(_roots.Keys);
            }

            return roots;
        }
                

        private void OnTimerElapsed(object source, ElapsedEventArgs e)
        {
            try
            {
                var commands = _commands.DumpCommands();

                if (commands.Count > 0)
                {
                    RunCommands(commands);
                }
                else
                {
                    Update();
                }
            }
            finally
            {
                _updateTimer.Start();
            }
        }

        private void RunCommands(HgCommandQueue commands)
        {
            var dirtyFilesList = new List<string>();

            foreach (var command in commands)
            {
                command.Run(this, dirtyFilesList);
            }

            if (dirtyFilesList.Count > 0)
            {
                Cache(Hg.GetFileInfo(dirtyFilesList.ToArray()));
            }

            OnStatusChanged();
        }

        protected virtual void Update()
        {
            long dirtyFilesCount = 0;
            double elapsed = 0;

            lock (_directoryWatchers.SyncRoot)
            {
                dirtyFilesCount = _directoryWatchers.DirtyFilesCount;
                elapsed = (DateTime.Now - _directoryWatchers.GetLatestChange()).TotalMilliseconds;
            }

            if (elapsed < UpdateInterval)
            {
                return;
            }

            if (_cacheUpdateRequired || dirtyFilesCount > 200)
            {
                UpdateCache();
                OnStatusChanged();
            }
            else if (dirtyFilesCount > 0)
            {
                var dirtyFiles = PrepareDirtyFiles();

                if (_cacheUpdateRequired || dirtyFiles.Length == 0)
                {
                    return;
                }

                try
                {
                    BeginUpdate();
                    Cache(Hg.GetFileInfo(dirtyFiles));
                }
                finally
                {
                    EndUpdate();
                }
                
                OnStatusChanged(dirtyFiles);
            }
        }

        private string[] PrepareDirtyFiles()
        {
            var dirtyFiles = new List<string>();

            foreach (var dirtyFile in _directoryWatchers.DumpDirtyFiles())
            {
                if (!PrepareDirtyFile(dirtyFile))
                {
                    continue;
                }
   
                if (_cacheUpdateRequired) // Can be set by PrepareWatchedFile
                {
                    break;
                }

                dirtyFiles.Add(dirtyFile);
            }

            return dirtyFiles.ToArray();
        }

        private bool PrepareDirtyFile(string fileName)
        {
            if (HgPath.IsDirectory(fileName))
            {
                return false;
            }

            if (fileName.IndexOf(".hg\\dirstate") != -1)
            {
                _cacheUpdateRequired = _updatingLevel == 0;
                return false;
            }
            
            if (fileName.IndexOf("\\.hg") != -1)
            {
                return false;
            }
            
            return HasChanged(fileName);
        }

        private bool HasChanged(string fileName)
        {
            var fileInfo = _cache[fileName];

            return fileInfo == null || fileInfo.HasChanged;
        }


        protected virtual void OnStatusChanged(string[] dirtyFiles)
        {
            OnStatusChanged();
        }

        protected virtual void OnStatusChanged()
        {
            StatusChanged(this, EventArgs.Empty);
        }
    }
}