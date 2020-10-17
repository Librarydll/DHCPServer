using OxyPlot;
using OxyPlot.Axes;
using Serilog;
using System;

namespace DHCPServer.Models.Infrastructure
{
	public class RoomLineGraphInfo : RoomInfo
	{

		//Колличество обращений 
		//сколько раз пытались записать данные
		//обращение происходят каждые 5 секунд так как изменяются два свойства проверка будет до 24
		private int _countRequest = -1;//значение -1 так как при создании объекиа оно обращается 1 раз
		public bool CanAdd => _countRequest >= 24;
		private bool isInvalid;
		public bool IsInvalid { get => isInvalid; private set { isInvalid = value; } }

		public bool IsAddedToGraph { get; set; }

		private ViewResolvingPlotModel _graphLineModel;
		public ViewResolvingPlotModel GraphLineModel

		{
			get { return _graphLineModel; }
			set { _graphLineModel = value; RaisePropertyChangedEvent(); }
		}

		private bool _temperatureLineVisibility = true;
		public bool TemperatureLineVisibility
		{
			get { return _temperatureLineVisibility; }
			set { _temperatureLineVisibility = value; RaisePropertyChangedEvent(); LineSeriesVisibilityChange(nameof(TemperatureLineVisibility), value); }
		}



		private bool _humidityLineVisibility = true;
		public bool HumidityLineVisibility
		{
			get { return _humidityLineVisibility; }
			set { _humidityLineVisibility = value; RaisePropertyChangedEvent(); LineSeriesVisibilityChange(nameof(HumidityLineVisibility), value); }
		}

		public RoomLineGraphInfo(RoomData roomData, Device device) : base(roomData, device)
		{
			GraphLineModel = ViewResolvingPlotModel.CreateDefault();
			TemperatureChangeEvent += TemperateChangedEventHandler;
			HumidityChangeEvent += HumidityChangedEventHandler;
		}

		public RoomLineGraphInfo(Device device):this(new RoomData(),device)
		{}

		public void SetInvalid(bool value)
		{
			if (value != IsInvalid)
			{
				IsInvalid = value;
				RaisePropertyChangedEvent("IsInvalid");
			}
		}

		private void TemperateChangedEventHandler(double value)
		{
			if (value < 0) return;

			//if (value != OldPositiveTemperatureValue)
			//{
			//	Date = DateTime.Now;
			//	Log.Logger.Information("ADDED To Graph DEVICE : {0} TEMPERATURE {1}", IPAddress, value);
			//	GraphLineModel.GetFirst().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), value));
			//	OldPositiveTemperatureValue = value;

			//}

		}
		private void HumidityChangedEventHandler(double value)
		{
			if (value < 0) return;

			//if (value != OldPositiveHumidityValue)
			//{
			//	Date = DateTime.Now;
			//	Log.Logger.Information("ADDED To Graph DEVICE : {0} HUMIDITY {1}", IPAddress, value);
			//	GraphLineModel.GetLast().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), value));
			//	OldPositiveHumidityValue = value;
			//}

		}

		private void LineSeriesVisibilityChange(string property, bool value)
		{
			if (GraphLineModel.Series != null)
			{
				if (property == nameof(TemperatureLineVisibility))
				{
					var t = GraphLineModel.GetFirst();
					if (value != t.IsVisible)
					{
						t.IsVisible = value;
					}
				}
				if (property == nameof(HumidityLineVisibility))
				{
					var h = GraphLineModel.GetLast();
					if (value != h.IsVisible)
					{
						h.IsVisible = value;
					}
				}
			}
		}


		public void AddToCollection()
		{
			if (Temperature < 0 || Humidity < 0) 
			{
				IsAddedToGraph = false;
				return;		
			}

			Date = DateTime.Now;
			AddToHumidity();
			AddToTemperature();
			IsAddedToGraph = true;
		}

		public void AddToHumidity()
		{
			
			Log.Logger.Information("ADDED To Graph DEVICE : {0} HUMIDITY {1}", Device?.IPAddress, Humidity);
			GraphLineModel.GetLast().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), Humidity));
		}
		public void AddToTemperature()
		{
			Log.Logger.Information("ADDED To Graph DEVICE : {0} TEMPERATURE {1}", Device?.IPAddress, Temperature);
			GraphLineModel.GetFirst().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), Temperature));
		}



	}

}
