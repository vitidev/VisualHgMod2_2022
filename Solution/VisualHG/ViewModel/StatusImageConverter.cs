using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using HgLib;
using VisualHg.Images;

namespace VisualHg.ViewModel
{
    public class StatusImageConverter : IValueConverter
    {
        private static readonly BitmapSource[] images =
            WpfImageLoader.GetStatusIcons(VisualHgOptions.Global.StatusImageFileName);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (ComparableStatus)value;
            var iconIndex = StatusImages.GetIndex((HgFileStatus)status);

            return images[iconIndex];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}