using System;
using System.Collections.Generic;
using System.Linq;

namespace HgLib
{
    public class HgRootDictionary
    {
        private Dictionary<string, string> _items;


        public object SyncRoot { get; private set; }

        public string[] Branches
        {
            get
            {
                lock (SyncRoot)
                {
                    return _items.Values.ToArray();
                }
            }
        }

        public string[] Roots
        {
            get
            {
                lock (SyncRoot)
                {
                    return _items.Keys.ToArray();
                }
            }
        }


        public HgRootDictionary()
        {
            SyncRoot = new object();
            _items = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }


        public void Update(string root)
        {
            if (String.IsNullOrEmpty(root))
            {
                return;
            }

            lock (SyncRoot)
            {
                _items[root] = Hg.GetCurrentBranchName(root);
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                _items.Clear();
            }
        }

        public string GetBranch(string path)
        {
            var branch = "";
            var root = HgPath.FindRepositoryRoot(path);

            lock (SyncRoot)
            {
                _items.TryGetValue(root, out branch);
            }

            return branch;
        }
    }
}