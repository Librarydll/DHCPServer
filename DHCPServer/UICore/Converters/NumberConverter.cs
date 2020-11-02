using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DHCPServer.UICore.Converters
{
	public class NumberConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var n = double.Parse(value.ToString()); 

			if (n == 0) return string.Empty;

			string mark = n > 0 ? "+" : "";
			return $"({mark}{n})";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
