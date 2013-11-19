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

        private int ignoreRequireUpdateAll;
        private bool updateAllRequired;
        private Timer updateTimer;
        private HgCommandQueue commands;
        private DirectoryWatcherMap directoryWatchers;


        public bool FileSystemWatch
        {
            set { directoryWatchers.FileSystemWatch = value; }
        }


        public HgRepository()
        {
            commands = new HgCommandQueue();
            directoryWatchers = new DirectoryWatcherMap();

            updateTimer = new Timer
            { 
                AutoReset = false,
                Interval = 100,
            };

            updateTimer.Elapsed += OnTimerElapsed;
            updateTimer.Start();
        }


        public void Dispose()
        {
            updateTimer.Dispose();
            directoryWatchers.Dispose();
        }

        protected override void AddRoot(string root)
        {
            base.AddRoot(root);

            directoryWatchers.WatchDirectory(root);
        }

        public override void Clear()
        {
            directoryWatchers.Clear();
            base.Clear();
        }

        public void Enqueue(HgCommand command)
        {
            commands.Enqueue(command);
        }
        

        private void OnTimerElapsed(object source, ElapsedEventArgs e)
        {
            try
            {
                var commandsToRun = commands.Dump();

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
                updateTimer.Start();
            }
        }


        private void RunCommands(HgCommand[] commands)
        {
            try
            {
                BeginUpdate();

                foreach (var command in commands)
                {
                    command.Run(this);
                }
            }
            finally
            {
                EndUpdate();
            }
        }


        protected virtual void Update()
        {
            if (updateAllRequired)
            {
                UpdateAll();
            }
            else
            {
                UpdateDirtyFiles();
            }
        }

        private void UpdateAll()
        {
            try
            {
                BeginUpdate();

                ClearCache();
                directoryWatchers.DumpDirtyFiles();

                foreach (var root in Roots)
                {
                    UpdateRootStatus(root);
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        private void UpdateDirtyFiles()
        {
            if (CanIgnoreDirtyFiles())
            {
                return;
            }

            var dirtyFiles = directoryWatchers.DumpDirtyFiles();

            if (HgDirstateChanged(dirtyFiles))
            {
                RequireUpdateAll();
            }
            else
            {
                UpdateDirtyFiles(dirtyFiles);
            }
        }

        private bool CanIgnoreDirtyFiles()
        {
            int dirtyFilesCount;
            double elapsed;

            lock (directoryWatchers.SyncRoot)
            {
                dirtyFilesCount = directoryWatchers.DirtyFilesCount;
                elapsed = (DateTime.Now - directoryWatchers.LatestChange).TotalMilliseconds;
            }

            return elapsed < UpdateInterval || dirtyFilesCount == 0;
        }

        private bool HgDirstateChanged(string[] dirtyFiles)
        {
            return dirtyFiles.Any(x => x.IndexOf(@".hg\dirstate") != -1);
        }

        private void UpdateDirtyFiles(string[] dirtyFiles)
        {
            var filesToUpdate = dirtyFiles.Where(FileChangeIsOfInterest).ToArray();

            if (filesToUpdate.Length > RequireUpdateAllFileLimit)
            {
                RequireUpdateAll();
            }
            else if (filesToUpdate.Length > 0)
            {
                UpdateFileStatus(dirtyFiles);
                OnStatusChanged(dirtyFiles);
            }
        }

        private bool FileChangeIsOfInterest(string fileName)
        {
            if (HgPath.IsDirectory(fileName))
            {
                return false;
            }
            
            if (fileName.IndexOf(@"\.hg") != -1)
            {
                return false;
            }
            
            return HasChanged(fileName);
        }

        private bool HasChanged(string fileName)
        {
            var fileInfo = GetFileInfo(fileName);

            return fileInfo == null || fileInfo.HasChanged;
        }
        

        protected void BeginUpdate()
        {
            ignoreRequireUpdateAll++;
            updateAllRequired = false;
        }

        protected void EndUpdate()
        {
            ignoreRequireUpdateAll = Math.Max(0, ignoreRequireUpdateAll - 1);

            if (ignoreRequireUpdateAll == 0)
            {
                OnStatusChanged();
            }
        }

        private void RequireUpdateAll()
        {
            updateAllRequired = (ignoreRequireUpdateAll == 0);
        }
    }
}