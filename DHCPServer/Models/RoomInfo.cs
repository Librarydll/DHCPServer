using DHCPServer.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models
{
	public class RoomInfo : BaseEntity, INotifyPropertyChanged
	{

		private string iPAddress;
		private double temperature;
		private double humidity;

		public string IPAddress { get => iPAddress; set { iPAddress = value; RaisePropertyChangedEvent(); } }
		public double Temperature { get => temperature; set { temperature = value; RaisePropertyChangedEvent(); } }
		public double Humidity { get => humidity; set { humidity = value; RaisePropertyChangedEvent(); } }

		public DateTime Date { get; set; }

		public RoomInfo(RoomData roomData, string address)
		{
			Temperature = roomData.Temperature;
			Humidity = roomData.Humidity;
			IPAddress = address;
			Date = DateTime.Now;
		}
		public RoomInfo()
		{

		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChangedEvent([CallerMemberName]string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}

	}
}
