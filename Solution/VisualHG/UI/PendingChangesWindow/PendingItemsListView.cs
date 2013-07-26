using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
  class PendingItemsListView : ListView
  {
    //array to cache items for the virtual list
    private ListViewItem[] _cache;
    //stores the index of the first item in the cache
    private int _firstItem;
    // pending files list
    public List<HgLib.HgFileStatusInfo> _list = new List<HgLib.HgFileStatusInfo>();
    // status images
    public ImageMapper _ImageMapper = new ImageMapper();
    // latest sorted column index
    int _previouslySortedColumn = -1;
    // remember selected files to restore selection
    SortOrder _SortOrder = SortOrder.Ascending;

    // ------------------------------------------------------------------------
    // construction - setup virtual list handler
    // ------------------------------------------------------------------------
    public PendingItemsListView()
    {
      this.SmallImageList = _ImageMapper.StatusImageList;

      //Hook up handlers for VirtualMode events.
      this.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(this_RetrieveVirtualItem);
      this.CacheVirtualItems += new CacheVirtualItemsEventHandler(this_CacheVirtualItems);
      this.SearchForVirtualItem += new SearchForVirtualItemEventHandler(this_SearchForVirtualItem);
      this.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this_ColumnClick);
    }

    // ------------------------------------------------------------------------
    // status item sorter callback routine
    // ------------------------------------------------------------------------
    public int compareInfoItem(HgLib.HgFileStatusInfo a, HgLib.HgFileStatusInfo b)
    {
      if (_SortOrder == SortOrder.Ascending)
      {
        if (_previouslySortedColumn <= 0)
          return a.fileName.CompareTo(b.fileName);
        else if (_previouslySortedColumn == 1)
          return a.fullPath.CompareTo(b.fullPath);
      }
      else
      {
        if (_previouslySortedColumn <= 0)
          return b.fileName.CompareTo(b.fileName);
        else if (_previouslySortedColumn == 1)
          return b.fullPath.CompareTo(b.fullPath);
      }
      return 0;
    }

    // ------------------------------------------------------------------------
    // sort content by column
    // ------------------------------------------------------------------------
    void SortByColumn(int mewColumn)
    {
      // store current selected items
      Dictionary<string, int> selection;
      StoreSelection(out selection);

      // toggle sort order and set column icon
      if (_previouslySortedColumn == mewColumn)
        _SortOrder = (_SortOrder == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
      else
        _SortOrder = SortOrder.Ascending;  

      ListViewSort.LVSort.SetSortIcons(this, ref _previouslySortedColumn, mewColumn);
      _previouslySortedColumn = mewColumn;

      // sort items and clear the cache
      _list.Sort(compareInfoItem);
      _cache = null;
      RestoreSelection(selection);
      this.Invalidate(false);
    }

    // ------------------------------------------------------------------------
    // update pending items list by status tracker object
    // ------------------------------------------------------------------------
    public void UpdatePendingList(HgStatusTracker tracker)
    {
      Dictionary<string, int> selection;
      StoreSelection(out selection);
        
      if (_previouslySortedColumn == -1)
        SortByColumn(0);
      
      // create new pending list ..
      List<HgLib.HgFileStatusInfo> newList;
      tracker.CreatePendingFilesList(out newList);
      newList.Sort(compareInfoItem);
      
      // .. and compare it to the current one
      bool somethingChanged = false;
      if (_list == null || newList.Count != _list.Count)
        somethingChanged = true;

      for (int pos = 0; !somethingChanged && pos < _list.Count; ++pos)
      {
        if (_list[pos].status != newList[pos].status)
          somethingChanged = true;
        if (_list[pos].fullPath.CompareTo(newList[pos].fullPath) != 0)
          somethingChanged = true;
      }

      // if we found changes between the lists, we now update the view
      if (somethingChanged)
      {
        // set new list into listview
        _list = newList;
        _cache = null;
        this.VirtualListSize = _list.Count;

        RestoreSelection(selection);
        this.Invalidate(false);
      }
    }

    // ------------------------------------------------------------------------
    // store current selected items to a map
    // ------------------------------------------------------------------------
    void StoreSelection(out Dictionary<string, int> selection)
    {
      selection = new Dictionary<string, int>();
      foreach (int index in SelectedIndices)
      {
        HgLib.HgFileStatusInfo info = _list[index];
        selection.Add(info.fullPath, 0);
      }
    }

    // ------------------------------------------------------------------------
    // restore given selection
    // ------------------------------------------------------------------------
    private void RestoreSelection(Dictionary<string, int> selection)
    {
        SelectedIndices.Clear();
        for (int pos = 0; pos < _list.Count; ++pos)
        {
            if (selection.ContainsKey(_list[pos].fullPath))
            {
                SelectedIndices.Add(pos);
            }
        }
    }

    // ------------------------------------------------------------------------
    // get item index by status
    // ------------------------------------------------------------------------
    int GetStateIcon(HgLib.HgFileStatus status)
    {
      switch (status)
      {
        case HgLib.HgFileStatus.scsMissing: return 5; // missing
        case HgLib.HgFileStatus.scsModified: return 1; // modified
        case HgLib.HgFileStatus.scsAdded: return 2; // added
        case HgLib.HgFileStatus.scsRemoved: return 4; // removed
        case HgLib.HgFileStatus.scsRenamed: return 3; // renamed
        case HgLib.HgFileStatus.scsCopied: return 6; // copied
        case HgLib.HgFileStatus.scsUncontrolled: return 5; // unknown
      }
      return 0;
    }

    // ------------------------------------------------------------------------
    // Dynamically returns a ListViewItem with the required properties;
    // ------------------------------------------------------------------------
    void this_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
    {
      //check to see if the requested item is currently in the cache
      if (_cache != null && e.ItemIndex >= _firstItem && e.ItemIndex < _firstItem + _cache.Length)
      {
        //A cache hit, so get the ListViewItem from the cache instead of making a new one.
        e.Item = _cache[e.ItemIndex - _firstItem];
      }
      else
      {
        //A cache miss, so create a new ListViewItem and pass it back.
        if (e.ItemIndex < _list.Count)
        {
          HgLib.HgFileStatusInfo info = _list[e.ItemIndex];
          e.Item = new ListViewItem(info.fileName);
          e.Item.ImageIndex = GetStateIcon(info.status);
          e.Item.SubItems.Add(info.fullPath);
        }
      }
    }

    // ------------------------------------------------------------------------
    // Manages the cache. ListView calls this when it might need a 
    // cache refresh.
    // ------------------------------------------------------------------------
    void this_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
    {
      //We've gotten a request to refresh the cache.
      //First check if it's really neccesary.
      if (_cache != null && e.StartIndex >= _firstItem && e.EndIndex <= _firstItem + _cache.Length)
      {
        //If the newly requested cache is a subset of the old cache, 
        //no need to rebuild everything, so do nothing.
        return;
      }

      // now we need to rebuild the cache.
      _firstItem = e.StartIndex;
      int length = e.EndIndex - e.StartIndex + 1; //indexes are inclusive
      _cache = new ListViewItem[length];

      for (int i = 0; i < length; i++)
      {
        int index = (i + _firstItem);
        if (index < _list.Count)
        {
          HgLib.HgFileStatusInfo info = _list[index];
          ListViewItem item = new ListViewItem(info.fileName);
          item.ImageIndex = GetStateIcon(info.status);
          item.SubItems.Add(info.fullPath);
          _cache[i] = item;
        }
      }
    }

    // ------------------------------------------------------------------------
    // This event handler enables search functionality, and is called
    // for every search request when in Virtual mode.
    // ------------------------------------------------------------------------
    void this_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
    {
      for (int pos = 0; pos < _list.Count; ++pos)
      {
        if (_list[pos].fileName.StartsWith(e.Text, StringComparison.OrdinalIgnoreCase))
        {
          e.Index = pos;
          break;
        }
      }
    }

    // ------------------------------------------------------------------------
    // column click handler - sort and upadte items
    // ------------------------------------------------------------------------
    void this_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      SortByColumn(e.Column);
    }
  }
}
