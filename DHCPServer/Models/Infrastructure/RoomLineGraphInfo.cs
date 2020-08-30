using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DHCPServer.Models.Infrastructure
{
	public class RoomLineGraphInfo:RoomInfo
	{

		private ObservableDataSource<Point> _temperaturePointDataSource =new ObservableDataSource<Point>();
		public ObservableDataSource<Point> TemperaturePointDataSource
		{
			get { return _temperaturePointDataSource; }
			set { _temperaturePointDataSource = value; RaisePropertyChangedEvent(); }
		}
		private ObservableDataSource<Point> _humidityPointDataSource = new ObservableDataSource<Point>();
		public ObservableDataSource<Point> HumidityPointDataSource
		{
			get { return _humidityPointDataSource; }
			set { _humidityPointDataSource = value; RaisePropertyChangedEvent(); }
		}
		public RoomLineGraphInfo()
		{
		}
		public RoomLineGraphInfo(RoomData roomData,string IpAddress):base(roomData, IpAddress)
		{
		}
		private void AddToTemperatureDataSource()
		{
			if (Temperature < 0) return;
			Date = new DateTime();
			var hour = Date.Hour;
			Point point = new Point(hour, Temperature);
			TemperaturePointDataSource.Collection.Add(point);
		}
		private void AddToHumidityDataSource()
		{
			if (Humidity < 0 ) return;
			Date = new DateTime();
			var hour = Date.Hour;
			Point point = new Point(hour, Humidity);
			HumidityPointDataSource.Collection.Add(point);
		}

		public override void RaisePropertyChangedEvent([CallerMemberName] string prop = "")
		{
			if (prop == nameof(Temperature))
			{
				AddToTemperatureDataSource();
			}
			if (prop == nameof(Humidity))
			{
				AddToHumidityDataSource();
			}
			base.RaisePropertyChangedEvent(prop);
		}

		public void AddToCollections()
		{
			AddToTemperatureDataSource();
			AddToHumidityDataSource();
		}

		//public void Initialize()
		//{
		//	Point p1 = new Point(11, 20);
		//	Point p3 = new Point(12, 25);
		//	Point p4 = new Point(13, 23);
		//	Point p5 = new Point(14, 22);
		//	TemperaturePointDataSource.Collection.Add(p1);
		//	TemperaturePointDataSource.Collection.Add(p3);
		//	TemperaturePointDataSource.Collection.Add(p4);
		//	TemperaturePointDataSource.Collection.Add(p5);
		//}
	}
}
