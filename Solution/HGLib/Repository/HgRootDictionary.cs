using System;
using System.Collections.Generic;
using System.Linq;

namespace HgLib.Repository
{
    internal class HgRootDictionary
    {
        private readonly Dictionary<string, string> items;


        public object SyncRoot { get; }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return items.Count;
                }
            }
        }

        public string[] Branches
        {
            get
            {
                lock (SyncRoot)
                {
                    return items.Values.ToArray();
                }
            }
        }

        public string[] Roots
        {
            get
            {
                lock (SyncRoot)
                {
                    return items.Keys.ToArray();
                }
            }
        }


        public HgRootDictionary()
        {
            SyncRoot = new object();
            items = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }


        public void Update(string root)
        {
            if (string.IsNullOrEmpty(root))
            {
                return;
            }

            lock (SyncRoot)
            {
                items[root] = Hg.GetCurrentBranchName(root);
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                items.Clear();
            }
        }

        public string GetBranch(string path)
        {
            var branch = "";
            var root = HgPath.FindRepositoryRoot(path);

            lock (SyncRoot)
            {
                items.TryGetValue(root, out branch);
            }

            return branch;
        }
    }
}