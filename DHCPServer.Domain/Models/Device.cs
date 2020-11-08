using Dapper.Contrib.Extensions;
using DHCPServer.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DHCPServer.Domain.Models
{
	public class Device : BaseEntity
	{
		private string iPAddress;
		public string IPAddress
		{
			get { return iPAddress; }
			set { SetProperty(ref iPAddress, value); }
		}

		private string nick;
		public string Nick
		{
			get { return nick; }
			set { SetProperty(ref nick, value); }
		}
	}
}
