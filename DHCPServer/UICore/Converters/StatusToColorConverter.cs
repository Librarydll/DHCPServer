using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DHCPServer.UICore.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        Brush Invalid =(SolidColorBrush) (new BrushConverter().ConvertFrom("#FF2400"));
        Brush Valid =(SolidColorBrush) (new BrushConverter().ConvertFrom("#66FF00"));
 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var isInvalid = bool.Parse(value.ToString());

            if (isInvalid)
                return Invalid;
            return Valid;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
