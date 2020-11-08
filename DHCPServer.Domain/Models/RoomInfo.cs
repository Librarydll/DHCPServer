using Dapper.Contrib.Extensions;
using DHCPServer.Domain.Models.Common;
using System;

namespace DHCPServer.Domain.Models
{
	public class RoomInfo : BaseEntity
	{
		private double temperature;
		private double humidity;
		private ActiveDevice device;

		public event Action<double> TemperatureChangeEvent;
		public event Action<double> HumidityChangeEvent;

		public double Temperature { get => temperature; set { temperature = value; RaisePropertyChangedEvent(); TemperatureChangeEvent?.Invoke(value); } }
		public double Humidity { get => humidity; set { humidity = value; RaisePropertyChangedEvent(); HumidityChangeEvent?.Invoke(value); } }

		public DateTime Date { get; set; }

		
		public int DeviceId { get; set; }
		[Computed]

		public ActiveDevice ActiveDevice { get => device; set { device = value; RaisePropertyChangedEvent(); } }

		public RoomInfo(RoomData roomData, ActiveDevice device)
		{
			Temperature = roomData.Temperature;
			Humidity = roomData.Humidity;
			ActiveDevice = device;
			Date = DateTime.Now;
			DeviceId = device.Id;
		}
		public RoomInfo(ActiveDevice device)
		{
			ActiveDevice = device;
			DeviceId = device.Id;
		}
		public RoomInfo()
		{

		}

		

	}
}
