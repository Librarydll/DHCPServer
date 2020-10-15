using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Core.Extensions
{
	public static class DateExtensions
	{
		public static DateTime ConvertToDateTime(this DateTime date)
		{
					
			return new DateTime(date.Ticks);
		}
		
	}
}
