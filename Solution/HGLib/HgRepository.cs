using System;
using System.Linq;
using System.Timers;
using HgLib.Repository;
using HgLib.Repository.Commands;

namespace HgLib
{
    public class HgRepository : HgRepositoryBase, IDisposable
    {
        private const int UpdateInterval = 2000;
        private const int RequireUpdateAllFileLimit = 200;

        private int _ignoreRequireUpdateAll;
        private bool _updateAllRequired;
        private readonly Timer _updateTimer;
        private readonly HgCommandQueue _commands;
        private readonly DirectoryWatcherMap _directoryWatchers;


        public HgRepository()
        {
            _commands = new HgCommandQueue();
            _directoryWatchers = new DirectoryWatcherMap();

            _updateTimer = new Timer
            {
                AutoReset = false,
                Interval = 100,
            };

            _updateTimer.Elapsed += OnTimerElapsed;
            _updateTimer.Start();
        }

        public virtual void AddFiles(params string[] fileNames)
        {
            Enqueue(new AddFilesHgCommand(fileNames));
        }

        public virtual void RemoveFiles(params string[] fileNames)
        {
            Enqueue(new RemoveFilesHgCommand(fileNames));
        }

        public virtual void RenameFiles(string[] fileNames, string[] newFileNames)
        {
            Enqueue(new RenameFilesHgCommand(fileNames, newFileNames));
        }

        public void UpdateFileStatus(params string[] fileNames)
        {
            Enqueue(new UpdateFileStatusHgCommand(fileNames));
        }

        public void UpdateRootStatus(string path)
        {
            Enqueue(new UpdateRootStatusHgCommand(path));
        }

        internal void AddFilesInternal(string[] fileNames)
        {
            AddFilesProtected(fileNames);
        }

        internal void RemoveFilesInternal(string[] fileNames)
        {
            RemoveFilesProtected(fileNames);
        }

        internal void RenameFilesInternal(string[] fileNames, string[] newFileNames)
        {
            RenameFilesProtected(fileNames, newFileNames);
        }

        internal void UpdateFileStatusInternal(string[] fileNames)
        {
            UpdateFileStatusProtected(fileNames);
        }

        internal void UpdateRootStatusInternal(string path)
        {
            UpdateRootStatusProtected(path);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _updateTimer.Dispose();
                _directoryWatchers.Dispose();
            }
        }

        protected override void AddRoot(string root)
        {
            base.AddRoot(root);

            _directoryWatchers.WatchDirectory(root);
        }

        public override void Clear()
        {
            _directoryWatchers.Clear();
            base.Clear();
        }


        private void OnTimerElapsed(object source, ElapsedEventArgs e)
        {
            try
            {
                var commandsToRun = _commands.Dump();

                if (commandsToRun.Length > 0)
                {
                    RunCommands(commandsToRun);
                }
                else
                {
                    Update();
                }
            }
            finally
            {
                RestartUpdateTimer();
            }
        }

        private void RestartUpdateTimer()
        {
            try
            {
                _updateTimer.Start();
            }
            catch (ObjectDisposedException)
            {
            }
        }


        private void Enqueue(HgCommand command)
        {
            _commands.Enqueue(command);
        }

        private void RunCommands(HgCommand[] commands)
        {
            try
            {
                BeginUpdate();

                foreach (var command in commands) 
                    command.Run(this);
            }
            finally
            {
                EndUpdate();
            }
        }


        protected virtual void Update()
        {
            if (_updateAllRequired)
                UpdateAll();
            else
                UpdateDirtyFiles();
        }

        private void UpdateAll()
        {
            try
            {
                BeginUpdate();

                ClearCache();
                _directoryWatchers.DumpDirtyFiles();

                foreach (var root in Roots) 
                    UpdateRootStatusProtected(root);
            }
            finally
            {
                EndUpdate();
            }
        }

        private void UpdateDirtyFiles()
        {
            if (CanIgnoreDirtyFiles())
                return;

            var dirtyFiles = _directoryWatchers.DumpDirtyFiles();

            if (HgDirstateChanged(dirtyFiles))
                RequireUpdateAll();
            else
                UpdateDirtyFiles(dirtyFiles);
        }

        private bool CanIgnoreDirtyFiles()
        {
            int dirtyFilesCount;
            double elapsed;

            lock (_directoryWatchers.SyncRoot)
            {
                dirtyFilesCount = _directoryWatchers.DirtyFilesCount;
                elapsed = (DateTime.Now - _directoryWatchers.LatestChange).TotalMilliseconds;
            }

            return elapsed < UpdateInterval || dirtyFilesCount == 0;
        }

        private bool HgDirstateChanged(string[] dirtyFiles)
        {
            return dirtyFiles.Any(x => x.IndexOf(@".hg\dirstate", StringComparison.Ordinal) != -1);
        }

        private void UpdateDirtyFiles(string[] dirtyFiles)
        {
            var filesToUpdate = dirtyFiles.Where(FileChangeIsOfInterest).ToArray();

            if (filesToUpdate.Length > RequireUpdateAllFileLimit)
                RequireUpdateAll();
            else if (filesToUpdate.Length > 0)
            {
                UpdateFileStatusProtected(dirtyFiles);
                OnStatusChanged();
            }
        }

        protected virtual bool FileChangeIsOfInterest(string fileName)
        {
            if (HgPath.IsDirectory(fileName))
                return false;

            if (fileName.IndexOf(@"\.hg", StringComparison.Ordinal) != -1)
                return false;

            return HasChanged(fileName);
        }

        private bool HasChanged(string fileName)
        {
            var fileInfo = GetFileInfo(fileName);

            return fileInfo == null || fileInfo.HasChanged;
        }


        protected void BeginUpdate()
        {
            _ignoreRequireUpdateAll++;
            _updateAllRequired = false;
        }

        protected void EndUpdate()
        {
            _ignoreRequireUpdateAll = Math.Max(0, _ignoreRequireUpdateAll - 1);

            if (_ignoreRequireUpdateAll == 0) 
                OnStatusChanged();
        }

        private void RequireUpdateAll()
        {
            _updateAllRequired = _ignoreRequireUpdateAll == 0;
        }
    }
}