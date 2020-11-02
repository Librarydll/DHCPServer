using Dapper.Contrib.Extensions;
using DHCPServer.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Domain.Models
{
	public class Report: BaseEntity
	{
		public Report()
		{
			ActiveDevices = new List<ActiveDevice>();
		}
		public string Title { get; set; }
		public DateTime LastUpdated { get; set; }

		public DateTime FromTime { get; set; }
		public DateTime ToTime => FromTime.AddDays(Days);

		public int DeviceId { get; set; }

		public int Days { get; set; }

		[Computed]
		public ICollection<ActiveDevice> ActiveDevices { get; set; }


	}
}
