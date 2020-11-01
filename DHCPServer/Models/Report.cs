using DHCPServer.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models
{
	public class Report: BaseEntity
	{
		public string Title { get; set; }
		public DateTime LastUpdated { get; set; }

		public DateTime FromTime { get; set; }
		public DateTime ToTime { get; set; }

		public int DeviceId { get; set; }

	}
}
