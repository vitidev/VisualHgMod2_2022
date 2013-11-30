using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using HgLib;
using Microsoft.VisualStudio.Shell;
using VisualHg.ViewModel;

namespace VisualHg.Controls
{
    public partial class PendingChangesView : UserControl
    {
        private PendingChangeSorter sorter;
        private PendingChanges pendingChanges;
        private double MinumumColumnWidth = 9;

        public PendingChangesView()
        {
            InitializeComponent();
            InitializePendingChanges();
            InitializeSorter();

            SetMenuItemImages();
            UpdateMenuItemsVisibility();

            listView.AddHandler(Thumb.DragDeltaEvent, (DragDeltaEventHandler)OnColumnThumbDragDelta, true);
        }

        private void InitializePendingChanges()
        {
            pendingChanges = new PendingChanges();
            listView.DataContext = pendingChanges;
        }

        private void InitializeSorter()
        {
            sorter = new PendingChangeSorter(listView);
        }

        public void Synchronize(HgFileInfo[] files)
        {
            pendingChanges.Synchronize(files);
        }


        private void OpenSelectedFiles()
        {
            foreach (var fileName in GetSelectedFiles())
            {
                try
                {
                    var serviceProvider = Package.GetGlobalService(typeof(IServiceProvider)) as IServiceProvider;

                    VsShellUtilities.OpenDocument(serviceProvider, fileName);
                }
                catch (Exception e)
                {
                    if (SingleItemSelected)
                    {
                        MessageBox.Show(e.Message, "Microsoft Visual Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OpenSelectedFiles(object sender, RoutedEventArgs e)
        {
            OpenSelectedFiles();
        }

        private void ShowCommitWindow(object sender, RoutedEventArgs e)
        {
            VisualHgDialogs.ShowCommitWindow(GetSelectedFiles());
        }

        private void ShowDiffWindow(object sender, RoutedEventArgs e)
        {
            if (SingleItemSelected)
            {
                VisualHgDialogs.ShowDiffWindow(GetSelectedFiles().First());
            }
        }

        private void ShowRevertWindow(object sender, RoutedEventArgs e)
        {
            VisualHgDialogs.ShowRevertWindow(GetSelectedFiles());
        }

        private void ShowHistoryWindow(object sender, RoutedEventArgs e)
        {
            if (SingleItemSelected)
            {
                VisualHgDialogs.ShowHistoryWindow(GetSelectedFiles().First());
            }
        }


        private void OnListViewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OpenSelectedFiles();
            }
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMenuItemsVisibility();
        }

        private void OnListViewColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            sorter.SortBy(e.OriginalSource as GridViewColumnHeader);
        }

        private void OnColumnThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = e.OriginalSource as Thumb;
            var header = thumb.TemplatedParent as GridViewColumnHeader;
            var column = header != null ? header.Column : null;

            if (column != null && column.ActualWidth < MinumumColumnWidth)
            {
                column.Width = MinumumColumnWidth;
            }
        }


        private void UpdateMenuItemsVisibility()
        {
            if (listView.SelectedItems.Count == 0)
            {
                listView.ContextMenu = null;
                return;
            }

            var status = GetAggregateSelectedItemsStatus();

            listView.ContextMenu = contextMenu;

            openMenuItem.Visibility = BoolToVisibility(!VisualHgFileStatus.Matches(status, HgFileStatus.Deleted));
            commitMenuItem.Visibility = BoolToVisibility(VisualHgFileStatus.Matches(status, HgFileStatus.Pending));
            revertMenuItem.Visibility = BoolToVisibility(VisualHgFileStatus.Matches(status, HgFileStatus.Pending));
            diffMenuItem.Visibility = BoolToVisibility(SingleItemSelected && VisualHgFileStatus.Matches(status, HgFileStatus.Comparable));
            historyMenuItem.Visibility = BoolToVisibility(SingleItemSelected && VisualHgFileStatus.Matches(status, HgFileStatus.Tracked));
        }


        private bool SingleItemSelected
        {
            get { return listView.SelectedItems.Count == 1; }
        }

        private string[] GetSelectedFiles()
        {
            return listView.SelectedItems.Cast<PendingChange>().Select(x => (string)x.FullName).ToArray();
        }

        private HgFileStatus GetAggregateSelectedItemsStatus()
        {
            if (listView.SelectedItems.Count == 0)
            {
                return HgFileStatus.None;
            }

            return listView.SelectedItems.Cast<PendingChange>().Select(x => (HgFileStatus)x.Status).Aggregate((x, y) => x | y);
        }

        private static Visibility BoolToVisibility(bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }


        private void SetMenuItemImages()
        {
            var images = ImageMapper.CreateMenuBitmapImages()
                .Select(x => new Image { Source = x })
                .ToArray();

            commitMenuItem.Icon = images[0];
            historyMenuItem.Icon = images[1];
            diffMenuItem.Icon = images[4];
            revertMenuItem.Icon = images[7];
            openMenuItem.Icon = images[8];
        }
    }
}