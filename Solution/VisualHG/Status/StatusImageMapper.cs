using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace VisualHG
{
    /// <summary>
    /// This enum specified the usage of glyph icons by VisualHG
    /// </summary>
    /// <remarks>This enum contains 16 members which should map the VsStateIcon class if possible</remarks>
    public enum HGGlyph
    {
        /// <summary>
        /// Path is versioned and modified (STATEICON_CHECKEDOUT /0x2)
        /// </summary>
        Modified,

        /// <summary>
        /// Item is versioned and unmodified (STATEICON_READONLY /0x6)
        /// </summary>
        Normal,

        /// <summary>
        /// File is versioned; but is not available on disk (STATEICON_DISABLED /0x7)
        /// </summary>
        FileMissing,

        /// <summary>
        /// File has been added but was never committed before (Last+1 /0xC)
        /// </summary>
        Added,

        /// <summary>
        /// File is in conflict; must be resolved before continuing (Last+3 /0xE)
        /// </summary>
        InConflict,
    }

    sealed class ImageMapper
    {
        ImageList _statusImageList;
        ImageList _menuImageList;

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
            using (Stream images = GetImageStream(fileName))
            {
                if (images == null)
                    return null;

                Image image = Image.FromStream(images, true, true);
                Bitmap bitmap = (Bitmap)image;

                ImageList imageList = new ImageList();

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
            Assembly assembly = Assembly.GetExecutingAssembly();
            string imagePath = Path.Combine(Path.GetDirectoryName(assembly.Location), fileName);

            if (File.Exists(imagePath))
            {
                return File.OpenRead(imagePath);
            }

            return assembly.GetManifestResourceStream(String.Concat(typeof(ImageMapper).Namespace, ".Resources.", fileName));
        }
    }
}
