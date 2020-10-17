using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Core.Extensions
{
	public static class DateExtensions
	{
		public static DateTime ToToday(this DateTime date)
		{
			var today = DateTime.Now;
			return new DateTime(today.Year, today.Month, today.Day, date.TimeOfDay.Hours, date.TimeOfDay.Minutes, date.TimeOfDay.Seconds);
		}
	}
}
