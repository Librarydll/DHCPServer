using DHCPServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DHCPServer.Services
{
	public interface IClientService
	{
		Task TryRecieve(CancellationToken cancellation,IEnumerable<Device> devices);
		event Action<RoomInfo, DeviceResponseStatus> ReciveMessageEvent;
		event Action<Device> ReciveMessageErrorEvent;

	}
}
