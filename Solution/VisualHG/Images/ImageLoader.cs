using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

namespace VisualHg.Images
{
    public static class ImageLoader
    {
        public static Bitmap GetBitmap(string fileName, string resourceName)
        {
            using (var stream = ImageLoader.GetImageStream(fileName, resourceName))
            {
                // The conversion is needed for image splitting 
                // (Bitmap.Clone throws OutOfMemoryException otherwise)
                return ConvertTo32bpp((Bitmap)Image.FromStream(stream));
            }
        }

        private static Bitmap ConvertTo32bpp(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
                bitmap.Save(stream, ImageFormat.Png);
         
                return (Bitmap)Image.FromStream(stream);
            }
        }

        public static Stream GetImageStream(string fileName, string resourceName)
        {
            return OpenFile(fileName) ?? GetResourceStream(resourceName);
        }

        private static Stream OpenFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            try
            {
                return File.OpenRead(fileName);
            }
            catch
            {
                return null;
            }
        }

        private static Stream GetResourceStream(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resource = String.Concat(typeof(VisualHgPackage).Namespace, ".Resources.", resourceName);

            return assembly.GetManifestResourceStream(resource);
        }
    }
}
