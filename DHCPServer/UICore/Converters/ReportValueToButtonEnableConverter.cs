using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DHCPServer.UICore.Converters
{
    public class ReportValueToButtonEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter?.ToString() == "check")
            {
                if (value == null)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }

            if (value == null)
                return Visibility.Hidden;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
