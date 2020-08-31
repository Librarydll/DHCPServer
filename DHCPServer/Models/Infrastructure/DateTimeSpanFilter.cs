using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.Infrastructure
{
	public class DateTimeSpanFilter
	{
		public DateTime FromDate { get; set; }
		public DateTime ToDate { get; set; }

		public TimeSpan FromTime { get; set; }
		public TimeSpan ToTime { get; set; }

		public bool IsTimeInclude { get; set; }

		public DateTimeSpanFilter()
		{
			FromDate = DateTime.Now;
			ToDate = DateTime.Now;
			FromTime = new TimeSpan(0, 0, 0);
			ToTime = new TimeSpan(10, 0, 0);
		}

		public bool Validate()
		{
			if (IsDateValidate() && IsTimeValidate()) return true;
			return false;
		}
		public bool IsDateValidate()
		{
			return	FromDate.Date <= ToDate.Date;
		}
		public bool IsTimeValidate()
		{
			return FromTime <= ToTime;
		}
	}
}
