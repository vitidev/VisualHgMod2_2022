using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HgLib;

namespace VisualHg.Images
{
    public static class StatusImages
    {
        public static int GetIndex(HgFileStatus status)
        {
            switch (status)
            {
                case HgFileStatus.Modified:
                    return 1;
                case HgFileStatus.Added:
                    return 2;
                case HgFileStatus.Removed:
                    return 3;
                case HgFileStatus.Clean:
                    return 4;
                case HgFileStatus.Missing:
                    return 5;
                case HgFileStatus.NotTracked:
                    return 6;
                case HgFileStatus.Ignored:
                    return 7;
                case HgFileStatus.Renamed:
                    return 8;
                case HgFileStatus.Copied:
                    return 9;
                default:
                    return 0;
            }
        }

        public static int GetIndexLimited(HgFileStatus status)
        {
            switch (status)
            {
                case HgFileStatus.Modified:
                    return 0;
                case HgFileStatus.Added:
                case HgFileStatus.Copied:
                case HgFileStatus.Renamed:
                    return 1;
                case HgFileStatus.Removed:
                    return 2;
                case HgFileStatus.Clean:
                    return 3;
                default:
                    return -1;
            }
        }


        public static ImageList Create(string fileName)
        {
            return Create(ImageConstants.StatusIconWidth, fileName, ImageConstants.StatusIconsResourceName);
        }

        public static ImageList CreateLimited(string fileName)
        {
            using (var imageList = Create(fileName))
            {
                var limitedImageList = new ImageList { ImageSize = imageList.ImageSize };

                limitedImageList.Images.AddRange(new [] 
                {
                    GetImage(imageList, HgFileStatus.Modified),
                    GetImage(imageList, HgFileStatus.Added),
                    GetImage(imageList, HgFileStatus.Removed),
                    GetImage(imageList, HgFileStatus.Clean),
                });

                return limitedImageList;
            }
        }

        private static Image GetImage(ImageList imageList, HgFileStatus status)
        {
            var iconIndex = GetIndex(status);

            return imageList.Images[iconIndex];
        }


        private static ImageList Create(int iconWidth, string fileName, string resourceName)
        {
            using (var imageStream = ImageLoader.GetImageStream(fileName, resourceName))
            {
                return GetImageList(iconWidth, imageStream);
            }
        }

        private static ImageList GetImageList(int iconWidth, Stream imageStream)
        {
            var image = Image.FromStream(imageStream, true, true);

            return GetImageList(iconWidth, (Bitmap)image);
        }

        private static ImageList GetImageList(int iconWidth, Bitmap bitmap)
        {
            var imageList = new ImageList
            {
                ImageSize = new Size(iconWidth, bitmap.Height),
                TransparentColor = bitmap.GetPixel(0, 0),
            };

            imageList.Images.AddStrip(bitmap);

            return imageList;
        }
    }
}
