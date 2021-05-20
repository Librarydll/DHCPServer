using DHCPServer.Domain.Models.Common;

namespace DHCPServer.Domain.Models
{
    public class DeviceSetting : BaseEntity
	{
		private double _temperatureRange;
		public double TemperatureRange
		{
			get { return _temperatureRange; }
			set { SetProperty(ref _temperatureRange, value); }
		}

		private double _humidityRange;
		public double HumidityRange
		{
			get { return _humidityRange; }
			set { SetProperty(ref _humidityRange, value); }
		}
        public int ActiveDeviceId { get; set; }
        public void SetSetting(double temp, double hum)
		{
			TemperatureRange = temp;
			HumidityRange = hum;
		}
	}
}
