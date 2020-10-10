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

		public double OldPositiveTemperatureValue { get; set; }
		public double OldPositiveHumidityValue { get; set; }

		//Колличество обращений 
		//сколько раз пытались записать данные
		//обращение происходят каждые 5 секунд так как изменяются два свойства проверка будет до 24
		private int _countRequest = -1;//значение -1 так как при создании объекиа оно обращается 1 раз
		public bool CanAdd => _countRequest >= 24;
		private bool isInvalid;
		public bool IsInvalid { get => isInvalid; private set { isInvalid = value; } }

		private bool humidityAdded = false;
		private bool temperatureAdded = false;

		private readonly OxyColor temperatureColor = OxyColors.Red;
		private readonly OxyColor humidityColor = OxyColors.Blue;
		private readonly MarkerType markerType = MarkerType.None;
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
			set { _humidityLineVisibility = value; RaisePropertyChangedEvent(); LineSeriesVisibilityChange(nameof(HumidityLineVisibility), value); }
		}

		public RoomLineGraphInfo(RoomData roomData, Device device) : base(roomData, device)
		{
			GraphLineModel = new ViewResolvingPlotModel();
			SetUpModel();
			_lineSeries = CreateLineSeries();
			GraphLineModel.Series.Add(_lineSeries.First());
			GraphLineModel.Series.Add(_lineSeries.Last());

			//TemperatureChangeEvent += TemperateChangedEventHandler;
		//	HumidityChangeEvent += HumidityChangedEventHandler;

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
				CanTrackerInterpolatePoints = true,
				Title = "Температура",
				Color = temperatureColor
			};
			var humidityLineSeries = new LineSeries
			{
				StrokeThickness = 2,
				MarkerSize = 3,
				MarkerStroke = humidityColor,
				MarkerType = markerType,
				CanTrackerInterpolatePoints = true,
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
			var dt = DateTime.Now;
			var dateAxis = new DateTimeAxis() 
			{
				MajorGridlineStyle = LineStyle.Solid,
				MinorGridlineStyle = LineStyle.Dot, 
				IntervalLength = 200,
				Position = AxisPosition.Bottom,
				Title = "Время",
				MajorStep = 1.0 / 24 ,
				StringFormat ="HH:mm",
				Minimum = DateTimeAxis.ToDouble(dt),
				Maximum = DateTimeAxis.ToDouble(dt.AddHours(6)),
				IntervalType = DateTimeIntervalType.Hours,

			};
			//dateAxis.AxisChanged += DateAxis_AxisChanged; ;
			//dateAxis.TransformChanged += DateAxis_TransformChanged;
			GraphLineModel.Axes.Add(dateAxis);
			var valueAxis = new LinearAxis() 
			{ 
				MajorGridlineStyle = LineStyle.Solid, 
				MinorGridlineStyle = LineStyle.Dot, 
				Title = "Значение", 
				Position = AxisPosition.Left,
				MinorStep =0.5, 
				MajorStep=0.2,
				MinimumMinorStep=0.5,
				MinimumMajorStep=3,
				IntervalLength=100,
				AbsoluteMaximum = 70, 
				AbsoluteMinimum = 0, 
			};
			//	valueAxis.AxisChanged += DateAxis_AxisChanged; ;
			//valueAxis.TransformChanged += DateAxis_TransformChanged;
			GraphLineModel.Axes.Add(valueAxis);
		}

		private void DateAxis_AxisChanged(object sender, AxisChangedEventArgs e)
		{
			
		}

		private void DateAxis_TransformChanged(object sender, EventArgs e)
		{
			
		}

		private void TemperateChangedEventHandler(double value)
		{
			if (Temperature > 0) OldPositiveTemperatureValue = Temperature;

			if (CanPropertyChange())
			{
				if (value > 0)
				{
					Date = DateTime.Now;
					Log.Logger.Information("DEVICE : {0} TEMPERATURE {1}", IPAddress, value);
					_lineSeries.First().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), value));
					temperatureAdded = true;
				}
			}

		}
		private void HumidityChangedEventHandler(double value)
		{
			if (value > 0) OldPositiveTemperatureValue = value;

			if (CanPropertyChange())
			{
				if (value > 0)
				{
					Date = DateTime.Now;
					Log.Logger.Information("DEVICE : {0} HUMIDITY {1}", IPAddress, value);
					_lineSeries.Last().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), value));
					humidityAdded = true;
				}
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

			if (Temperature < 0 || Humidity < 0) return false;

			_countRequest += 1;

			if (CanAdd)
			{
				if (humidityAdded && temperatureAdded)
				{
					humidityAdded = false;
					temperatureAdded = false;
					_countRequest = 0;
				}
				return true;
			}

			return false;
		}

		public void AddToCollections()
		{
			Date = DateTime.Now;

			AddToTemperatureLine();
			AddToHumidityLine();

		}

		private void AddToTemperatureLine()
		{
			double t;
			if (Temperature > 0)
			{
				t = Temperature;
			}
			else
			{
				t = OldPositiveTemperatureValue;
			}

			Log.Logger.Information("ADDED TO Graph DEVICE : {0} TEMPERATURE {1}", IPAddress, t);
			_lineSeries.First().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), t));

		}
		private void AddToHumidityLine()
		{
			double h;
			if (Humidity > 0)
			{
				h = Humidity;
			}
			else
			{
				h = OldPositiveHumidityValue;
			}

			Log.Logger.Information("ADDED TO Graph DEVICE : {0} TEMPERATURE {1}", IPAddress, h);
			_lineSeries.Last().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), h));
		}
	}

}
