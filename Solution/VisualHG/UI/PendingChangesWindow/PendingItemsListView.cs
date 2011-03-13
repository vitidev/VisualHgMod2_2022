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

namespace VisualHG
{
  class PendingItemsListView : ListView
  {
    private ListViewItem[] _cache; //array to cache items for the virtual list
    private int _firstItem; //stores the index of the first item in the cache
    StatusImageMapper _statusImages = new StatusImageMapper();
    // empty pendinf files list
    public List<HGLib.HGFileStatusInfo> _list = new List<HGLib.HGFileStatusInfo>();
    
    public PendingItemsListView()
    {
        //Create a simple ListView.
        this.SmallImageList = _statusImages.StatusImageList;
        
        //Hook up handlers for VirtualMode events.
        this.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(this_RetrieveVirtualItem);
        this.CacheVirtualItems += new CacheVirtualItemsEventHandler(this_CacheVirtualItems);
        this.SearchForVirtualItem += new SearchForVirtualItemEventHandler(this_SearchForVirtualItem);
    }

      public int compareAscanding(HGLib.HGFileStatusInfo a, HGLib.HGFileStatusInfo b)
      {
        return a.fileName.CompareTo(b.fileName);
      }

      public void UpdatePendingList(HGStatusTracker tracker)
      {
        List<HGLib.HGFileStatusInfo> newList;
        tracker.CreatePendingFilesList(out newList);
        newList.Sort(compareAscanding);
        
        bool somethingChanged = false; 
        if (_list == null || newList.Count != _list.Count)
          somethingChanged=true;

        for (int pos = 0; !somethingChanged && pos < _list.Count; ++pos)
        {
          if(_list[pos].state != newList[pos].state)
            somethingChanged = true;
          if (_list[pos].fullPath.CompareTo(newList[pos].fullPath)!=0)
            somethingChanged = true;
        }   

        if (somethingChanged)
        {
          _list = newList;
          _cache = null; // clear cache
          this.VirtualListSize = _list.Count;
          this.Invalidate(false);
        }
      }
      

    int GetStateIcon(char state)
    {
      switch(state)
      {
        case 'M': return 1;
        case 'A': return 2;
        case 'R': return 4; //TODO: create removed icon
        case 'N': return 3; // renamed
        case 'P': return 3; // copied
        case '?': return 2; // unknown
      }
      return 0;
    }
    
    //Dynamically returns a ListViewItem with the required properties;
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
              HGLib.HGFileStatusInfo info = _list[e.ItemIndex];
              e.Item = new ListViewItem(info.fileName);
              e.Item.ImageIndex = GetStateIcon(info.state);
              e.Item.SubItems.Add(info.fullPath);
            }   
        }
    }

    //Manages the cache. ListView calls this when it might need a 
    //cache refresh.
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

        //Now we need to rebuild the cache.
        _firstItem = e.StartIndex;
        int length = e.EndIndex - e.StartIndex + 1; //indexes are inclusive
        _cache = new ListViewItem[length];

        for (int i = 0; i < length; i++)
        {
          int index = (i + _firstItem);
          if (index < _list.Count)
          {
            HGLib.HGFileStatusInfo info = _list[index];
            ListViewItem item = new ListViewItem(info.fileName);
            item.ImageIndex = GetStateIcon(info.state);
            item.SubItems.Add(info.fullPath);
            _cache[i] = item;
          }  
        }
    }

    //This event handler enables search functionality, and is called
    //for every search request when in Virtual mode.
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
  }
}
