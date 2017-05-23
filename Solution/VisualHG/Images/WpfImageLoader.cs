using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace VisualHg.Images
{
    public static class WpfImageLoader
    {
        public static BitmapImage[] GetStatusIcons(string fileName)
        {
            using (var bitmaps = new BitmapList(LoadStatusIcons(fileName)))
            {
                return bitmaps.Select(ToBitmapImage).ToArray();
            }
        }

        public static BitmapImage[] GetMenuIcons()
        {
            using (var bitmaps = new BitmapList(LoadMenuIcons()))
            {
                return bitmaps.Select(ToBitmapImage).ToArray();
            }
        }

        
        private static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                return BitmapImageFrom(stream);
            }
        }

        private static BitmapImage BitmapImageFrom(MemoryStream stream)
        {
            var image = new BitmapImage();

            image.BeginInit();
            image.StreamSource = stream;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();

            return image;
        }


        private static Bitmap[] LoadMenuIcons()
        {
            return LoadIcons(ImageConstants.MenuIconWidth, "", ImageConstants.MenuIconsResourceName);
        }

        private static Bitmap[] LoadStatusIcons(string fileName)
        {
            return LoadIcons(ImageConstants.StatusIconWidth, fileName, ImageConstants.StatusIconsResourceName);
        }

        private static Bitmap[] LoadIcons(int iconWidth, string fileName, string resourceName)
        {
            using (var bitmap = ImageLoader.GetBitmap(fileName, resourceName))
            {
                return Split(bitmap, iconWidth);
            }
        }
        
        private static Bitmap[] Split(Bitmap bitmap, int width)
        {
            try
            {
                return Enumerable.Range(0, bitmap.Width / width)
                    .Select(x => new Rectangle(x * width, 0, width, bitmap.Height))
                    .Select(x => bitmap.Clone(x, bitmap.PixelFormat))
                    .ToArray();
            }
            catch
            {
                return null;
            }
        }


        private class BitmapList : IEnumerable<Bitmap>, IDisposable
        {
            private readonly IList<Bitmap> _items;

            public BitmapList(Bitmap[] items)
            {
                _items = items;
            }

            public void Dispose()
            {
                foreach (var item in _items)
                {
                    item.Dispose();
                }
            }

            public IEnumerator<Bitmap> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _items.GetEnumerator();
            }
        }
    }
}
