using DHCPServer.Domain.Models.Common;

namespace DHCPServer.Domain.Models
{
    public class Device : BaseEntity
	{

		public Device()
		{

		}
		public Device(string ipAddress,string nick)
		{
			iPAddress = ipAddress;
			Nick = nick;
		}
		public Device(Device device):this(device.IPAddress,device.Nick)
		{

		}
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
