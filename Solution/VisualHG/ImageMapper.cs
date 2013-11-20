using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using HgLib;

namespace VisualHg
{
    public class ImageMapper
    {
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
