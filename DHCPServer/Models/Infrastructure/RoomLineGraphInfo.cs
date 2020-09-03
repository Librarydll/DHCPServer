using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Serilog;
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

		private readonly OxyColor temperatureColor = OxyColors.Red;
		private readonly OxyColor humidityColor = OxyColors.Blue;
		private readonly MarkerType markerType = MarkerType.Circle;
		private LineSeries[] _lineSeries = null;

		private ViewResolvingPlotModel _graphLineModel;
		public ViewResolvingPlotModel GraphLineModel
		{
			get { return _graphLineModel; }
			set { _graphLineModel = value; RaisePropertyChangedEvent(); }
		}
		public RoomLineGraphInfo()
		{

		}
		public RoomLineGraphInfo(RoomData roomData, Device device) : base(roomData, device)
		{
			GraphLineModel = new ViewResolvingPlotModel();
			SetUpModel();
			_lineSeries = CreateLineSeries();
			GraphLineModel.Series.Add(_lineSeries.First());
			GraphLineModel.Series.Add(_lineSeries.Last());
		}
		private void AddToTemperatureDataSource()
		{
			var x = CreatePointX();
			Log.Logger.Information("DEVICE : {0} TEMPERATURE {1}, Time {2}", IPAddress, Temperature, x);
			_lineSeries.First().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), Humidity));
			_count++;
		}
		private void AddToHumidityDataSource()
		{
			var x = CreatePointX();
			Log.Logger.Information("DEVICE : {0} HUMIDITY {1}, Time {2}", IPAddress, Humidity, x);
			_lineSeries.Last().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), Humidity));
			_count++;
		}

		public override void RaisePropertyChangedEvent([CallerMemberName] string prop = "")
		{
			_countRequest += 1;

			if (CanAdd)
			{
				if (Temperature > 0 && Humidity > 0)
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

		private LineSeries[] CreateLineSeries()
		{
			var temperatureLineSeries = new LineSeries
			{
				StrokeThickness = 2,
				MarkerSize = 3,
				MarkerStroke = temperatureColor,
				MarkerType = markerType,
				CanTrackerInterpolatePoints = false,
				Title = "Температура",
				Color =temperatureColor
			};
			var humidityLineSeries = new LineSeries
			{
				StrokeThickness = 2,
				MarkerSize = 3,
				MarkerStroke = humidityColor,
				MarkerType = markerType,
				CanTrackerInterpolatePoints = false,
				Title = "Влажность",
				Color = humidityColor
			};

			return new LineSeries[]
			{
				temperatureLineSeries,humidityLineSeries
			};
		}
		private void SetUpModel()
		{
			GraphLineModel.LegendTitle = "Данные";
			GraphLineModel.LegendOrientation = LegendOrientation.Horizontal;
			GraphLineModel.LegendPlacement = LegendPlacement.Outside;
			GraphLineModel.LegendPosition = LegendPosition.TopRight;
			GraphLineModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
			GraphLineModel.LegendBorder = OxyColors.Black;

			var dateAxis = new DateTimeAxis() { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, IntervalLength = 80, Position = AxisPosition.Bottom, Title = "Время"};
			GraphLineModel.Axes.Add(dateAxis);
			var valueAxis = new LinearAxis() { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Значение", Position = AxisPosition.Left, StartPosition = 0,Minimum=0};
			GraphLineModel.Axes.Add(valueAxis);
		}
	}
}
