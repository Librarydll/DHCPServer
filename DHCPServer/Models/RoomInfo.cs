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
		private Device device;

		public event Action<double> TemperatureChangeEvent;
		public event Action<double> HumidityChangeEvent;

		public string IPAddress { get => iPAddress; set { iPAddress = value; RaisePropertyChangedEvent(); } }
		public double Temperature { get => temperature; set { temperature = value; RaisePropertyChangedEvent(); TemperatureChangeEvent?.Invoke(value); } }
		public double Humidity { get => humidity; set { humidity = value; RaisePropertyChangedEvent(); HumidityChangeEvent?.Invoke(value); } }

		public DateTime Date { get; set; }

		public Device Device { get => device; set { device = value; RaisePropertyChangedEvent(); } }

		public RoomInfo(RoomData roomData, Device device)
		{
			Temperature = roomData.Temperature;
			Humidity = roomData.Humidity;
			Device = device;
			IPAddress = Device?.IPAddress;
			Date = DateTime.Now;
		}
		public RoomInfo()
		{

		}

		public event PropertyChangedEventHandler PropertyChanged;

		public virtual void RaisePropertyChangedEvent([CallerMemberName]string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}

	}
}
