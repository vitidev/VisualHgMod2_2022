using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg.ViewModel
{
    public class FileTypeImageConverter : IValueConverter
    {
        private IVsImageService imageService;

        public FileTypeImageConverter()
        {
            imageService = Package.GetGlobalService(typeof(SVsImageService)) as IVsImageService;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fileName = (string)(ComparablePath)value;
            var icon = imageService.GetIconForFile(fileName, __VSUIDATAFORMAT.VSDF_WPF);
            
            return GetData(icon);
        }

        private static object GetData(IVsUIObject icon)
        {
            object data;
            
            icon.get_Data(out data);

            return data;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
