using System.Windows.Forms;
using HgLib;

namespace VisualHg.Controls
{
    public class HgFileInfoListViewItem : ListViewItem
    {
        public const int ColumnCount = 4;
        public const int FileNameColumn = 0;
        public const int StatusColumn = 1;
        public const int RootNameColumn = 2;
        public const int PathColumn = 3;

        public HgFileInfoListViewItem(HgFileInfo fileInfo)
        {
            ImageIndex = ImageMapper.GetStatusIconIndex(fileInfo.Status);

            Text = GetText(fileInfo, 0);

            for (int i = 1; i < ColumnCount; i++)
            {
                SubItems.Add(GetText(fileInfo, i));
            }
        }

        public static string GetText(HgFileInfo fileInfo, int column)
        {
            switch (column)
            {
                case StatusColumn:
                    return fileInfo.Status.ToString();
                case RootNameColumn:
                    return fileInfo.RootName;
                case PathColumn:
                    return fileInfo.Name;
                default:
                    return fileInfo.ShortName;
            }
        }
    }
}