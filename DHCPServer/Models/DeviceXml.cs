using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DHCPServer.Models
{
	[Serializable]
	public class DeviceXml
	{
		[XmlElement(ElementName = "Device")]
		public Device[] Devices { get; set; }
	}
}
