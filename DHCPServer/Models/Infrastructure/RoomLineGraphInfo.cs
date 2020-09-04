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
			set { _humidityLineVisibility = value; RaisePropertyChangedEvent(); LineSeriesVisibilityChange(nameof(TemperatureLineVisibility), value); }
		}

		public RoomLineGraphInfo(RoomData roomData, Device device) : base(roomData, device)
		{
			GraphLineModel = new ViewResolvingPlotModel();
			SetUpModel();
			_lineSeries = CreateLineSeries();
			GraphLineModel.Series.Add(_lineSeries.First());
			GraphLineModel.Series.Add(_lineSeries.Last());

			TemperatureChangeEvent += TemperateChangedEventHandler;
			HumidityChangeEvent += HumidityChangedEventHandler;


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
				Color = temperatureColor
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

			var dateAxis = new DateTimeAxis() { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, IntervalLength = 80, Position = AxisPosition.Bottom, Title = "Время" };
			GraphLineModel.Axes.Add(dateAxis);
			var valueAxis = new LinearAxis() { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Значение", Position = AxisPosition.Left, StartPosition = 0, Minimum = 0 };
			GraphLineModel.Axes.Add(valueAxis);
		}

		private void TemperateChangedEventHandler(double value)
		{
			if (CanPropertyChange())
			{
				Date = DateTime.Now;
				Log.Logger.Information("DEVICE : {0} TEMPERATURE {1}, Time {2}", IPAddress, Temperature, Date.TimeOfDay.ToString("hh:mm::ss"));
				_lineSeries.First().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), Humidity));
				_count++;
			}

		}
		private void HumidityChangedEventHandler(double value)
		{
			if (CanPropertyChange())
			{
				Date = DateTime.Now;
				Log.Logger.Information("DEVICE : {0} HUMIDITY {1}, Time {2}", IPAddress, Humidity, Date.TimeOfDay.ToString("hh:mm::ss"));
				_lineSeries.Last().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), Humidity));
				_count++;
			}
		}

		private void LineSeriesVisibilityChange(string property, bool value)
		{
			if (_lineSeries != null)
			{
				if (property == nameof(TemperatureLineVisibility))
				{
					var t = _lineSeries.First();
					if (value != t.IsVisible)
					{
						t.IsVisible = value;
					}
				}
				if (property == nameof(HumidityLineVisibility))
				{
					var h = _lineSeries.Last();
					if (value != h.IsVisible)
					{
						h.IsVisible = value;
					}
				}
			}
		}


		private bool CanPropertyChange()
		{
			_countRequest += 1;

			if (CanAdd)
			{
				if (_count == 2)
				{
					_count = 0;
					_countRequest = 0;
				}
				return true;
			}

			return false;
		}
	}

}
