using DHCPServer.Core.Events.Model;
using DHCPServer.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Core.Events
{
	class ApplicationEvents
	{
	}

	public class DeviceUpdateEvent :PubSubEvent<DeviceEventModel> { }
}
