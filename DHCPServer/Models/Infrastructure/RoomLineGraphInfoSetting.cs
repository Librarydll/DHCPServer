using DHCPServer.Domain.Models.Common;

namespace DHCPServer.Models.Infrastructure
{
	public class RoomLineGraphInfoSetting: BaseEntity
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
	}
}
