using DHCPServer.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DHCPServer.Models
{
	public class Device : BaseEntity, IDevice
	{
		private string iPAddress;
		private string nick;

		public string IPAddress { get => iPAddress; set { iPAddress = value; RaisePropertyChangedEvent(); } }

		public string Nick { get => nick; set { nick = value; RaisePropertyChangedEvent(); } }

	}
}
