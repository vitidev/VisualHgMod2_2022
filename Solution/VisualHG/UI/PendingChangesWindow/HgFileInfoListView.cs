using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using HgLib;

namespace VisualHg
{
    public class HgFileInfoListView : ListView
    {
        private List<HgFileInfo> files;
        private IList<HgFileInfo> readOnlyFiles;

        private ListViewItemCache cache;
        private HgFileInfoComparer comparer;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList<HgFileInfo> Files
        {
            get { return readOnlyFiles; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                BeginUpdate();
                var selectedPaths = SelectedFiles;

                files = new List<HgFileInfo>(value);
                files.Sort(comparer);
                readOnlyFiles = files.AsReadOnly();
                VirtualListSize = files.Count;
                ClearVirtualItemsCache();

                RestoreSelection(selectedPaths);
                EndUpdate();
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] SelectedFiles
        {
            get { return SelectedIndices.Cast<int>().Select(x => Files[x].FullName).ToArray(); }
        }


        public HgFileInfoListView()
        {
            ClearVirtualItemsCache();

            comparer = new HgFileInfoComparer();
         
            SmallImageList = new ImageMapper().StatusImageList;

            CacheVirtualItems += (s, e) => UpdateCache(e.StartIndex, e.EndIndex);
            RetrieveVirtualItem += (s, e) => e.Item = GetItem(e.ItemIndex);
            SearchForVirtualItem += (s, e) => e.Index = FindItemStartingWith(e.Text);
            ColumnClick += (s, e) => SortByColumn(e.Column);
        }


        private void RestoreSelection(string[] paths)
        {
            SelectedIndices.Clear();

            foreach (var index in GetItemIndices(paths))
            {
                SelectedIndices.Add(index);
            }
        }

        private IEnumerable<int> GetItemIndices(string[] paths)
        {
            for (int i = 0; i < Files.Count; i++)
            {
                if (paths.Contains(Files[i].FullName))
                {
                    yield return i;
                }
            }
        }


        private void ClearVirtualItemsCache()
        {
            cache = ListViewItemCache.Empty;
        }

        private void UpdateCache(int startIndex, int endIndex)
        {
            if (cache.Contains(startIndex, endIndex))
            {
                return;
            }

            cache = new ListViewItemCache(startIndex, GetItems(startIndex, endIndex));
        }

        private ListViewItem[] GetItems(int startIndex, int endIndex)
        {
            return Enumerable.Range(startIndex, endIndex - startIndex).Select(x => GetItem(x)).ToArray();
        }

        private ListViewItem GetItem(int index)
        {
            if (cache.Contains(index))
            {
                return cache.GetItem(index);
            }
            else if (index < Files.Count)
            {
                return CreateListViewItem(index);
            }

            return null;
        }

        private ListViewItem CreateListViewItem(int index)
        {
            var file = Files[index];

            var listViewItem = new ListViewItem {
                Text = file.Name,
                ImageIndex = ImageMapper.GetStatusIconIndex(file.Status),
            };

            listViewItem.SubItems.Add(file.FullName);

            return listViewItem;
        }


        private int FindItemStartingWith(string text)
        {
            var index = -1;

            for (int i = 0; i < Files.Count; ++i)
            {
                var file = Files[i];

                if (file.Name.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }


        private void SortByColumn(int columnIndex)
        {
            int currentColumnToSort = comparer.ColumnToSort;

            comparer.SortByColumn(columnIndex);
            Sorting = comparer.Sorting;

            ListViewColumnIcons.Update(this, currentColumnToSort, columnIndex);

            files.Sort(comparer);

            ClearVirtualItemsCache();
            Invalidate();
        }
    }
}