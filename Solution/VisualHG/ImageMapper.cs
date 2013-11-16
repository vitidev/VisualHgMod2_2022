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
        private ImageList _statusImageList;
        private ImageList _menuImageList;

        public ImageList StatusImageList
        {
            get { return _statusImageList ?? (_statusImageList = CreateStatusImageList()); }
        }

        public ImageList MenuImageList
        {
            get { return _menuImageList ?? (_menuImageList = CreateMenuImageList()); }
        }

        public ImageList CreateStatusImageList()
        {
            return CreateImageList("StatusIcons.bmp", 7);
        }


        public ImageList CreateMenuImageList()
        {
            return CreateImageList("MenuIcons.bmp", 16);
        }


        private static ImageList CreateImageList(string fileName, int imageWidth)
        {
            using (var imageStream = GetImageStream(fileName))
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

        private static Stream GetImageStream(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyDirectory = Path.GetDirectoryName(assembly.Location);
            var imagePath = Path.Combine(assemblyDirectory, fileName);

            if (File.Exists(imagePath))
            {
                return File.OpenRead(imagePath);
            }

            return assembly.GetManifestResourceStream(String.Concat(typeof(ImageMapper).Namespace, ".Resources.", fileName));
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
