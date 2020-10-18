﻿using Dapper.Contrib.Extensions;
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
	public class RoomInfo : BaseEntity
	{
		private double temperature;
		private double humidity;
		private Device device;

		public event Action<double> TemperatureChangeEvent;
		public event Action<double> HumidityChangeEvent;

		public double Temperature { get => temperature; set { temperature = value; RaisePropertyChangedEvent(); TemperatureChangeEvent?.Invoke(value); } }
		public double Humidity { get => humidity; set { humidity = value; RaisePropertyChangedEvent(); HumidityChangeEvent?.Invoke(value); } }

		public DateTime Date { get; set; }

		public int DeviceId { get; set; }
		[Computed]

		public Device Device { get => device; set { device = value; RaisePropertyChangedEvent(); } }

		public RoomInfo(RoomData roomData, Device device)
		{
			Temperature = roomData.Temperature;
			Humidity = roomData.Humidity;
			Device = device;
			Date = DateTime.Now;
			DeviceId = device.Id;
		}
		public RoomInfo(Device device)
		{
			Device = device;
			DeviceId = device.Id;
		}
		public RoomInfo()
		{

		}

		

	}
}