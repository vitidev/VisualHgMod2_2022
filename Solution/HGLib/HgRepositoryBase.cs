using System;
using System.Linq;
using HgLib.Repository;

namespace HgLib
{
    public abstract class HgRepositoryBase
    {
        private HgFileInfoDictionary cache;
        private HgRootDictionary roots;


        public bool IsEmpty
        {
            get { return roots.Count == 0; }
        }

        public string[] Roots
        {
            get { return roots.Roots; }
        }

        public string[] Branches
        {
            get { return roots.Branches; }
        }

        public virtual HgFileInfo[] PendingFiles
        {
            get { return cache.PendingFiles; }
        }


        public event EventHandler StatusChanged = (s, e) => { };


        public HgRepositoryBase()
        {
            cache = new HgFileInfoDictionary();
            roots = new HgRootDictionary();
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
            cache.Remove(fileNames.Concat(newFileNames));
            Cache(Hg.RenameFiles(fileNames, newFileNames));
        }

        protected void UpdateFileStatusProtected(string[] fileNames)
        {
            Cache(Hg.GetFileInfo(fileNames));
        }

        protected void UpdateRootStatusProtected(string path)
        {
            var root = HgPath.FindRepositoryRoot(path);

            if (String.IsNullOrEmpty(root))
            {
                return;
            }

            AddRoot(root);

            Cache(Hg.GetRootStatus(root));
        }

        
        public string GetBranch(string path)
        {
            return roots.GetBranch(path);
        }

        public HgFileInfo GetFileInfo(string fileName)
        {
            return cache[fileName];
        }

        public HgFileStatus GetFileStatus(string fileName)
        {
            var fileInfo = cache[fileName];

            return fileInfo != null ? fileInfo.Status : HgFileStatus.NotTracked;
        }


        public virtual void Clear()
        {
            cache.Clear();
            roots.Clear();
        }


        protected virtual void AddRoot(string root)
        {
            roots.Update(root);
        }

        protected void Cache(HgFileInfo[] files)
        {
            cache.Add(files);
        }

        protected void ClearCache()
        {
            cache.Clear();
        }


        protected virtual void OnStatusChanged()
        {
            StatusChanged(this, EventArgs.Empty);
        }
    }
}