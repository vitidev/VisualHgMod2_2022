using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using HgLib;

namespace VisualHg.ViewModel
{
    public class StatusImageConverter : IValueConverter
    {
        private BitmapImage[] images;

        public StatusImageConverter()
        {
            images = ImageMapper.CreateStatusBitmapImages(VisualHgOptions.Global.StatusImageFileName);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (ComparableStatus)value;
            var iconIndex = ImageMapper.GetStatusIconIndex((HgFileStatus)status);

            return images[iconIndex];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
