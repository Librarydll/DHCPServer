using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DHCPServer.Models
{	
	[Serializable]
	public class Device
	{	
		[XmlIgnore]
		public int Id { get; set; }

		[XmlElement("IPAddress")]
		public string IPAddress { get; set; }

		[XmlElement("Nick")]
		public string Nick { get; set; }

	}
}
