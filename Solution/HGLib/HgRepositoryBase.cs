using System;
using System.Linq;
using HgLib.Repository;

namespace HgLib
{
    public abstract class HgRepositoryBase
    {
        private readonly HgFileInfoDictionary _cache;
        private readonly HgRootDictionary _roots;


        public bool IsEmpty => _roots.Count == 0;

        public string[] Roots => _roots.Roots;

        public string[] Branches => _roots.Branches;

        public virtual HgFileInfo[] PendingFiles => _cache.PendingFiles;


        public event EventHandler StatusChanged = (s, e) => { };


        protected HgRepositoryBase()
        {
            _cache = new HgFileInfoDictionary();
            _roots = new HgRootDictionary();
        }


        protected void AddFilesProtected(string[] fileNames)
        {
            Cache(Hg.AddFiles(fileNames, HgFileStatus.NotTracked | HgFileStatus.Removed));
        }

        protected void RemoveFilesProtected(string[] fileNames)
        {
            Cache(Hg.RemoveFiles(fileNames));
        }

        protected void RenameFilesProtected(string[] fileNames, string[] newFileNames)
        {
            _cache.Remove(fileNames.Concat(newFileNames));
            Cache(Hg.RenameFiles(fileNames, newFileNames));
        }

        protected void UpdateFileStatusProtected(string[] fileNames)
        {
            Cache(Hg.GetFileInfo(fileNames));
        }

        protected void UpdateRootStatusProtected(string path)
        {
            var root = HgPath.FindRepositoryRoot(path);

            if (string.IsNullOrEmpty(root))
                return;

            AddRoot(root);

            Cache(Hg.GetRootStatus(root));
        }


        public string GetBranch(string path)
        {
            return _roots.GetBranch(path);
        }

        public HgFileInfo GetFileInfo(string fileName)
        {
            return _cache[fileName];
        }

        public HgFileStatus GetFileStatus(string fileName)
        {
            var fileInfo = _cache[fileName];

            return fileInfo?.Status ?? HgFileStatus.NotTracked;
        }


        public virtual void Clear()
        {
            _cache.Clear();
            _roots.Clear();
        }


        protected virtual void AddRoot(string root)
        {
            _roots.Update(root);
        }

        protected void Cache(HgFileInfo[] files)
        {
            _cache.Add(files);
        }

        protected void ClearCache()
        {
            _cache.Clear();
        }


        protected virtual void OnStatusChanged()
        {
            StatusChanged(this, EventArgs.Empty);
        }
    }
}