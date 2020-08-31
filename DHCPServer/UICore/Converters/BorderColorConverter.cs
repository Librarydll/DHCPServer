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
	public class BorderColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool isInvalid = (bool)value;

			if (isInvalid)
			{
				return Brushes.Red;
			}
			else
			{
				return Brushes.DarkGreen;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Brushes.Gray;
		}
	}
}
