using Dapper.Contrib.Extensions;
using DHCPServer.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models
{
	public class DevicesList:BaseEntity, IDevice
	{
		public int DeviceId { get; set; }
		[Computed]
		public Device Device { get; set; }

	}
}
