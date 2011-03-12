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
    private ListViewItem[] myCache; //array to cache items for the virtual list
    private int firstItem; //stores the index of the first item in the cache
    StatusImageMapper _statusImages = new StatusImageMapper();
    public List<HGLib.HGFileStatusInfo> _list = null;
    
    public PendingItemsListView()
    {
        //Create a simple ListView.
        this.SmallImageList = _statusImages.StatusImageList;
        
        //Hook up handlers for VirtualMode events.
        this.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(this_RetrieveVirtualItem);
        this.CacheVirtualItems += new CacheVirtualItemsEventHandler(this_CacheVirtualItems);
        this.SearchForVirtualItem += new SearchForVirtualItemEventHandler(this_SearchForVirtualItem);
        
        /*
        //Search for a particular virtual item.
        //Notice that we never manually populate the collection!
        //If you leave out the SearchForVirtualItem handler, this will return null.
        ListViewItem lvi = this.FindItemWithText("111111");

        //Select the item found and scroll it into view.
        if (lvi != null)
        {
            this.SelectedIndices.Add(lvi.Index);
            this.EnsureVisible(lvi.Index);
        }
         */
    }

      public void UpdatePendingList(HGStatusTracker tracker)
      {
        this.SuspendLayout(); 
        this.VirtualListSize = 0;
        tracker.CreatePendingFilesList(out _list);
        myCache = null; // clear cache
        this.VirtualListSize = _list.Count;
        this.ResumeLayout();
        
      }

    int GetStateIcon(char state)
    {
      switch(state)
      {
        case 'M': return 1;
        case 'A': return 2;
        case 'R': return 1;
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
        if (myCache != null && e.ItemIndex >= firstItem && e.ItemIndex < firstItem + myCache.Length)
        {
            //A cache hit, so get the ListViewItem from the cache instead of making a new one.
            e.Item = myCache[e.ItemIndex - firstItem];
        }
        else
        {
            //A cache miss, so create a new ListViewItem and pass it back.
            HGLib.HGFileStatusInfo info = _list[e.ItemIndex];
            e.Item = new ListViewItem(info.FileName());
            e.Item.ImageIndex = GetStateIcon(info.state);
            e.Item.SubItems.Add(info.caseSensitiveFileName);
        }
    }
    ListViewItem item = null;
    

    //Manages the cache. ListView calls this when it might need a 
    //cache refresh.
    void this_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
    {
        //We've gotten a request to refresh the cache.
        //First check if it's really neccesary.
        if (myCache != null && e.StartIndex >= firstItem && e.EndIndex <= firstItem + myCache.Length)
        {
            //If the newly requested cache is a subset of the old cache, 
            //no need to rebuild everything, so do nothing.
            return;
        }

        //Now we need to rebuild the cache.
        firstItem = e.StartIndex;
        int length = e.EndIndex - e.StartIndex + 1; //indexes are inclusive
        myCache = new ListViewItem[length];

        //int x = 0;
        for (int i = 0; i < length; i++)
        {
          int index = (i + firstItem);
          HGLib.HGFileStatusInfo info = _list[index];
          item = new ListViewItem(info.FileName());
          item.ImageIndex = GetStateIcon(info.state);
          item.SubItems.Add(info.caseSensitiveFileName);
          myCache[i] = item;
            
        }

    }

    //This event handler enables search functionality, and is called
    //for every search request when in Virtual mode.
    void this_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
    {
        //We've gotten a search request.
        //In this example, finding the item is easy since it's
        //just the square of its index.  We'll take the square root
        //and round.
        double x = 0;
        if (Double.TryParse(e.Text, out x)) //check if this is a valid search
        {
            x = Math.Sqrt(x);
            x = Math.Round(x);
            e.Index = (int)x;

        }
        //If e.Index is not set, the search returns null.
        //Note that this only handles simple searches over the entire
        //list, ignoring any other settings.  Handling Direction, StartIndex,
        //and the other properties of SearchForVirtualItemEventArgs is up
        //to this handler.
    }
  }
}
