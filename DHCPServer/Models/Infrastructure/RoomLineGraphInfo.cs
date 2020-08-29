using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DHCPServer.Models.Infrastructure
{
	public class RoomLineGraphInfo:RoomInfo
	{
		private ObservableDataSource<Point> _temperaturePointDataSource;
		public ObservableDataSource<Point> TemperaturePointDataSource
		{
			get { return _temperaturePointDataSource; }
			set { _temperaturePointDataSource = value; RaisePropertyChangedEvent(); }
		}
		private ObservableDataSource<Point> _humidityPointDataSource;
		public ObservableDataSource<Point> HumidityPointDataSource
		{
			get { return _humidityPointDataSource; }
			set { _humidityPointDataSource = value; RaisePropertyChangedEvent(); }
		}
		public PenDescription TemperatureDescription { get; set; } = new PenDescription("Температура");
		public PenDescription HumidityDescription { get; set; } = new PenDescription("Влажность");
		public RoomLineGraphInfo()
		{
			TemperaturePointDataSource = new ObservableDataSource<Point>();
			HumidityPointDataSource = new ObservableDataSource<Point>();
		}
		public RoomLineGraphInfo(RoomData roomData,string IpAddress):base(roomData, IpAddress)
		{
			TemperaturePointDataSource = new ObservableDataSource<Point>();
			HumidityPointDataSource = new ObservableDataSource<Point>();
		}


		public void AddToTemperatureDataSource()
		{
			Date = new DateTime();
			var hour = Date.Hour;
			Point point = new Point(hour, Temperature);
			TemperaturePointDataSource.Collection.Add(point);
		}
		public void AddToHumidityDataSource()
		{
			Date = new DateTime();
			var hour = Date.Hour;
			Point point = new Point(hour, Humidity);
			HumidityPointDataSource.Collection.Add(point);
		}

		//public void Initialize()
		//{
		//	Point p1 = new Point(10, 20);
		//	Point p2 = new Point(11, 22);
		//	Point p3 = new Point(12, 25);
		//	Point p4 = new Point(13, 23);
		//	Point p5 = new Point(14, 22);
		//	TemperaturePointDataSource.Collection.Add( p1);
		//	TemperaturePointDataSource.Collection.Add( p2);
		//	TemperaturePointDataSource.Collection.Add( p3);
		//	TemperaturePointDataSource.Collection.Add( p4);
		//	TemperaturePointDataSource.Collection.Add( p5);
		//}
	}
}
