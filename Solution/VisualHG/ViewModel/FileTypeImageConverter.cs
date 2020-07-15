using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg.ViewModel
{
    public class FileTypeImageConverter : IValueConverter
    {
        private readonly object imageService;

        public FileTypeImageConverter()
        {
            var serviceType = VisualStudioShell11.GetType("Microsoft.VisualStudio.Shell.Interop.SVsImageService");

            if (serviceType != null)
            {
                imageService = Package.GetGlobalService(serviceType);
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (imageService == null)
            {
                return null;
            }

            try
            {
                var icon = GetIconForFile((string)(ComparablePath)value);

                return GetData(icon);
            }
            catch
            {
                return null;
            }
        }

        private IVsUIObject GetIconForFile(string fileName)
        {
            var type = imageService.GetType();

            var getIconMethod = type.GetMethod("GetIconForFile", new[] {typeof(string), typeof(__VSUIDATAFORMAT)});

            var icon = getIconMethod.Invoke(imageService, new object[] {fileName, __VSUIDATAFORMAT.VSDF_WPF});

            return (IVsUIObject)icon;
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