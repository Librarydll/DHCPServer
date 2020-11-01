using DHCPServer.Domain.Models;
using DHCPServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Core.Events.Model
{
	public class DeviceEventModel
	{
		public Device OldValue { get; set; }
		public Device NewValue { get; set; }
	}
}
