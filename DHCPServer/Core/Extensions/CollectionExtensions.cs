using DHCPServer.Domain.Models;
using DHCPServer.Models;
using DHCPServer.Models.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace DHCPServer.Core.Extensions
{
	public static class CollectionextEnsioncs
	{
		public static IEnumerable<DeviceClient> ToDeviceClient(this IEnumerable<ActiveDevice> devices)
		{
			return devices.Select(x => new DeviceClient(x)).ToList();
		}

		public static IEnumerable<RoomLineGraphInfo> ToRoomLineGraphInfo(this IEnumerable<RoomInfo> collection)
		{
			foreach (var room in collection)
			{
				yield return new RoomLineGraphInfo(new RoomData {
					Temperature = room.Temperature,
					Humidity = room.Humidity
				},
					room.ActiveDevice);
			}
		}

		public static void DisposeRange(this IEnumerable<DeviceClient> deviceClients)
        {
            foreach (var device in deviceClients)
            {
				device?.Dispose();
            }
        }

		public static IEnumerable<ActiveDevice> CreateActiveDevices(this IEnumerable<Device> devices,IEnumerable<ActiveDevice> activeDevices)
        {

            foreach (var device in devices)
            {
				var ad = activeDevices.FirstOrDefault(x => x.IPAddress == device.IPAddress);
                if (ad != null)
                {
					yield return ad;
                }
                else
                {
					yield return new ActiveDevice(device);        
                }
            }
        }


	}
}
