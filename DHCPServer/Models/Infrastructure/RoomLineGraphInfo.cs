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
	public class RoomLineGraphInfo : RoomInfo
	{
		//Колличество обращений 
		//сколько раз пытались записать данные
		//обращение происходят каждые 5 секунд так как изменяются два свойства проверка будет до 24
		private int _countRequest = -1;//значение -1 так как при создании объекиа оно обращается 1 раз
		private int _count = 0;
		public bool CanAdd => _countRequest >= 24;
		private bool isInvalid;
		public bool IsInvalid { get => isInvalid; private set { isInvalid = value; } }
		private ObservableDataSource<Point> _temperaturePointDataSource = new ObservableDataSource<Point>();
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
		public RoomLineGraphInfo(RoomData roomData, string IpAddress) : base(roomData, IpAddress)
		{
		}
		private void AddToTemperatureDataSource()
		{
			var x = CreatePointX();
			Point point = new Point(x, Temperature);
			TemperaturePointDataSource.Collection.Add(point);
			_count++;
		}
		private void AddToHumidityDataSource()
		{
			var x = CreatePointX();
			Point point = new Point(x, Humidity);
			HumidityPointDataSource.Collection.Add(point);
			_count++;
		}

		public override void RaisePropertyChangedEvent([CallerMemberName] string prop = "")
		{
			_countRequest += 1;

			if (CanAdd)
			{
				if (Temperature>0&&Humidity>0)
				{
					if (prop == nameof(Temperature))
					{
						AddToTemperatureDataSource();
					}
					if (prop == nameof(Humidity))
					{
						AddToHumidityDataSource();
					}
					if (_count == 2)
					{
						_count = 0;
						_countRequest = 0;
					} 
				}
			}

			base.RaisePropertyChangedEvent(prop);
		}

		public void AddToCollections()
		{
			AddToTemperatureDataSource();
			AddToHumidityDataSource();
		}

		public double CreatePointX()
		{
			Date = DateTime.Now;
			var hours = Date.Hour;
			var minutes = Date.Minute;
			string s = hours.ToString() + "," + minutes.ToString();
			return double.Parse(s);
		}

		public void SetInvalid(bool value)
		{
			if (value != IsInvalid)
			{
				IsInvalid = value;
				RaisePropertyChangedEvent("IsInvalid");
			}
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
