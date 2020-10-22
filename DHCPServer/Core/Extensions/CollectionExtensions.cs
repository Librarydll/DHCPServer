using DHCPServer.Models;
using DHCPServer.Models.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Core.Extensions
{
	public static class CollectionextEnsioncs
	{
		public static IEnumerable<DeviceClient> ToDeviceClient(this IEnumerable<Device> devices)
		{
			return devices.Select(x => new DeviceClient(x));
		}

		public static IEnumerable<RoomLineGraphInfo> ToRoomLineGraphInfo(this IEnumerable<RoomInfo> collection)
		{
			foreach (var room in collection)
			{
				yield return new RoomLineGraphInfo(new RoomData {
					Temperature = room.Temperature,
					Humidity = room.Humidity
				},
					room.Device);
			}
		}

		public static IEnumerable<Device> CheckDevice(this IEnumerable<Device> newDevices,IEnumerable<Device> oldDevices)
		{
			foreach (var device in newDevices)
			{
				var d = oldDevices.FirstOrDefault(x => x.Id == device.Id);

				if (d != null)
				{
					device.IsAdded = true;
				}
			}

			return newDevices;
		}


	}
}
