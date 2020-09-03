using DHCPServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DHCPServer.Services
{
	public class XmlDeviceProvider
	{
		private static readonly string path = "device.xml";
		private XmlSerializer serializer = new XmlSerializer(typeof(DeviceXml));
		public XmlDeviceProvider()
		{
		}

		public IEnumerable<Device> GetDevices()
		{
			if (File.Exists(path))
				return DeSerialize();
			return new List<Device>();
		}

		public IEnumerable<RoomInfo> CastDevices(IEnumerable<Device> devices)
		{
			ICollection<RoomInfo> roomInfos = new List<RoomInfo>();
			if (devices == null) return roomInfos;

			foreach (var device in devices)
			{
				roomInfos.Add(new RoomInfo(new RoomData(), device));
			}
			return roomInfos;
		}

		public void SaveDevices(IEnumerable<Device> devices)
		{
			CreateFile();
			Serialize(devices);
		}

		private IEnumerable<Device> DeSerialize()
		{
			var deviceXml = new DeviceXml();
			using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
			{
				try
				{
					deviceXml = (DeviceXml)serializer.Deserialize(stream);
				}
				catch (Exception ex)
				{
					return null;
				}
			}
			return deviceXml.Devices;
		}

		private void Serialize(IEnumerable<Device> devices)
		{
			try
			{
				var deviceXml = new DeviceXml();
				deviceXml.Devices = devices.ToArray();
				using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					serializer.Serialize(stream, deviceXml);
				}
			}
			catch (Exception ex)
			{
				return;
			}
		}
		public static void CreateFile()
		{
			if (!File.Exists(path))
			{
				File.Create(path);
			}
		}
	}
}
