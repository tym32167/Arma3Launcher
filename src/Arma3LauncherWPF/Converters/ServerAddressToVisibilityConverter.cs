using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Arma3LauncherWPF.Config;

namespace Arma3LauncherWPF.Converters
{
    public class ServerAddressToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var data = value as ServerAddress;
            if (data!=null)
            {
                if (!string.IsNullOrEmpty(data.IP)) return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
