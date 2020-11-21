using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Domain.Models
{
	public class MultiDevice:RoomInfo
	{
		public MultiDevice()
		{

		}
		public MultiDevice(ActiveDevice device):base(device)
		{}

		private double _temperatureMiddle;
		[JsonProperty("tMid")]
		public double TemperatureMiddle
		{
			get { return _temperatureMiddle; }
			set { SetProperty(ref _temperatureMiddle, value); }
		}
		private double _humidityMiddle;
		[JsonProperty("hMid")]
		public double HumidityMiddle
		{
			get { return _humidityMiddle; }
			set { SetProperty(ref _humidityMiddle, value); }
		}


		private double _temperatureNord;
		[JsonProperty("tNord")]
		public double TemperatureNord
		{
			get { return _temperatureNord; }
			set { SetProperty(ref _temperatureNord, value); }
		}
		private double _humidityNord;
		[JsonProperty("hNord")]
		public double HumidityNord
		{
			get { return _humidityNord; }
			set { SetProperty(ref _humidityNord, value); }
		}

		private double _temperatureProcess;
		[JsonProperty("tProcess")]

		public double TemperatureProcess
		{
			get { return _temperatureProcess; }
			set { SetProperty(ref _temperatureProcess, value); }
		}
		private double _humidityProcess;
		[JsonProperty("hProcess")]

		public double HumidityProcess
		{
			get { return _humidityProcess; }
			set { SetProperty(ref _humidityProcess, value); }
		}
	}
}
