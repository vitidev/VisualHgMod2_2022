using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using HgLib;

namespace VisualHg
{
    public class ImageMapper
    {
        public static BitmapImage[] CreateStatusBitmapImages(string fileName)
        {
            using (var imageList = ImageMapper.CreateStatusImageList(fileName))
            {
                return BitmapImagesFrom(imageList);
            }
        }

        public static BitmapImage[] CreateMenuBitmapImages()
        {
            using (var imageList = ImageMapper.CreateMenuImageList())
            {
                return BitmapImagesFrom(imageList);
            }
        }

        private static BitmapImage[] BitmapImagesFrom(ImageList imageList)
        {
            return imageList.Images.Cast<Bitmap>().Select(ToBitmapImage).ToArray();
        }

        private static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            var image = new BitmapImage();

            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
            }

            return image;
        }

        
        public static ImageList CreateStatusImageList(string fileName)
        {
            return CreateImageList(7, "StatusIcons.bmp", fileName);
        }

        public static ImageList CreateMenuImageList()
        {
            return CreateImageList(16, "MenuIcons.bmp");
        }


        private static ImageList CreateImageList(int imageWidth, string resourceName, string fileName = "")
        {
            using (var imageStream = GetImageStream(resourceName, fileName))
            {
                if (imageStream == null)
                {
                    return null;
                }

                var image = Image.FromStream(imageStream, true, true);
                var bitmap = (Bitmap)image;

                var imageList = new ImageList();

                imageList.ImageSize = new Size(imageWidth, bitmap.Height);
                bitmap.MakeTransparent(bitmap.GetPixel(0, 0));

                try
                {
                    imageList.Images.AddStrip(bitmap);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.ToString());
                }

                return imageList;
            }
        }

        private static Stream GetImageStream(string resourceName, string fileName)
        {
            if (File.Exists(fileName))
            {
                return File.OpenRead(fileName);
            }

            var assembly = Assembly.GetExecutingAssembly();
            var resource = String.Concat(typeof(ImageMapper).Namespace, ".Resources.", resourceName);

            return assembly.GetManifestResourceStream(resource);
        }


        public static int GetStatusIconIndex(HgFileStatus status)
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
    }
}
