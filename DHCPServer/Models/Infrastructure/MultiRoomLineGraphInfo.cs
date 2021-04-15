using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure.Common;
using OxyPlot;
using OxyPlot.Axes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DHCPServer.Models.Infrastructure
{
	public class MultiRoomLineGraphInfo : RoomLineBase<ActiveDevice, MultiRoomInfo>
	{

		private ViewResolvingPlotModel _graphLineModelForDefault;
		public ViewResolvingPlotModel GraphLineModelForDefault

		{
			get { return _graphLineModelForDefault; }
			set { _graphLineModelForDefault = value; RaisePropertyChangedEvent(); }
		}
		private ViewResolvingPlotModel _graphLineModelForMiddle;
		public ViewResolvingPlotModel GraphLineModelForMiddle

		{
			get { return _graphLineModelForMiddle; }
			set { _graphLineModelForMiddle = value; RaisePropertyChangedEvent(); }
		}
		private ViewResolvingPlotModel _graphLineModelForProcess;
		public ViewResolvingPlotModel GraphLineModelForProcess

		{
			get { return _graphLineModelForProcess; }
			set { _graphLineModelForProcess = value; RaisePropertyChangedEvent(); }
		}

		private ViewResolvingPlotModel _graphLineModelForNord;
		public ViewResolvingPlotModel GraphLineModelForProcessForNord

		{
			get { return _graphLineModelForNord; }
			set { _graphLineModelForNord = value; RaisePropertyChangedEvent(); }
		}

		private bool _temperatureLineVisibilityForDefault = true;
		public bool TemperatureLineVisibilityForDefault
		{
			get { return _temperatureLineVisibilityForDefault; }
			set { SetProperty(ref _temperatureLineVisibilityForDefault, value); LineSeriesVisibilityChange(nameof(TemperatureLineVisibilityForDefault), value); }
		}
		private bool _temperatureLineVisibilityForMiddle = true;
		public bool TemperatureLineVisibilityForMiddle
		{
			get { return _temperatureLineVisibilityForMiddle; }
			set { SetProperty(ref _temperatureLineVisibilityForMiddle, value); LineSeriesVisibilityChange(nameof(TemperatureLineVisibilityForMiddle), value); }
		}
		private bool _temperatureLineVisibilityForNord = true;
		public bool TemperatureLineVisibilityForNord
		{
			get { return _temperatureLineVisibilityForNord; }
			set { SetProperty(ref _temperatureLineVisibilityForNord, value); LineSeriesVisibilityChange(nameof(TemperatureLineVisibilityForNord), value); }
		}

		private bool _temperatureLineVisibilityForProcess = true;
		public bool TemperatureLineVisibilityForProcess
		{
			get { return _temperatureLineVisibilityForProcess; }
			set { SetProperty(ref _temperatureLineVisibilityForProcess, value); LineSeriesVisibilityChange(nameof(TemperatureLineVisibilityForProcess), value); }
		}




		private bool _HumidityLineVisibilityForDefault = true;
		public bool HumidityLineVisibilityForDefault
		{
			get { return _HumidityLineVisibilityForDefault; }
			set { SetProperty(ref _HumidityLineVisibilityForDefault, value); LineSeriesVisibilityChange(nameof(HumidityLineVisibilityForDefault), value); }
		}
		private bool _HumidityLineVisibilityForMiddle = true;
		public bool HumidityLineVisibilityForMiddle
		{
			get { return _HumidityLineVisibilityForMiddle; }
			set { SetProperty(ref _HumidityLineVisibilityForMiddle, value); LineSeriesVisibilityChange(nameof(HumidityLineVisibilityForMiddle), value); }
		}
		private bool _HumidityLineVisibilityForNord = true;
		public bool HumidityLineVisibilityForNord
		{
			get { return _HumidityLineVisibilityForNord; }
			set { SetProperty(ref _HumidityLineVisibilityForNord, value); LineSeriesVisibilityChange(nameof(HumidityLineVisibilityForNord), value); }
		}

		private bool _HumidityLineVisibilityForProcess = true;
		public bool HumidityLineVisibilityForProcess
		{
			get { return _HumidityLineVisibilityForProcess; }
			set { SetProperty(ref _HumidityLineVisibilityForProcess, value); LineSeriesVisibilityChange(nameof(HumidityLineVisibilityForProcess), value); }
		}

		public MultiRoomLineGraphInfo(ActiveDevice device,bool startTimer =true) : base(device)
		{
			GraphLineModelForDefault = ViewResolvingPlotModel.CreateDefault();
			GraphLineModelForMiddle = ViewResolvingPlotModel.CreateDefault();
			GraphLineModelForProcessForNord = ViewResolvingPlotModel.CreateDefault();
			GraphLineModelForProcess = ViewResolvingPlotModel.CreateDefault();
			if (startTimer)
				_timer = new Timer(_timer_Tick, null, new TimeSpan(0, 10, 0), new TimeSpan(0, 10, 0));
		}

		private void _timer_Tick(object state)
		{
			if (AddToCollection())
			{
				OnCollectionAdded();
				if (_timerIntervalChanged)
				{
					_timer.Change(new TimeSpan(0, 10, 0), new TimeSpan(0, 10, 0));
					_timerIntervalChanged = false;
				}

			}
			else
			{
				_timer.Change(new TimeSpan(0, 1, 0), new TimeSpan(0, 1, 0));
				_timerIntervalChanged = true;
			}
		}

		public bool AddToCollection()
		{
			if (!IsInvalid)
			{
				RoomInfo.Date = DateTime.Now;
				AddToHumidity();
				AddToTemperature();
				return true;
			}
			return false;
		}

		public void AddToHumidity()
		{
			Log.Logger.Information("ADDED To 3 Graphs DEVICE : {0} HumidityDefault {1}, HumidityMid {2}, HumidityProcess {3}", ActiveDevice?.IPAddress,
				RoomInfo.Humidity,RoomInfo.HumidityMiddle,RoomInfo.HumidityProcess);
			GraphLineModelForDefault.GetLast().Points.Add(new DataPoint(DateTimeAxis.ToDouble(RoomInfo.Date), RoomInfo.Humidity));
			GraphLineModelForMiddle.GetLast().Points.Add(new DataPoint(DateTimeAxis.ToDouble(RoomInfo.Date), RoomInfo.HumidityMiddle));
			GraphLineModelForProcess.GetLast().Points.Add(new DataPoint(DateTimeAxis.ToDouble(RoomInfo.Date), RoomInfo.HumidityProcess));
		}
		public void AddToTemperature()
		{
			Log.Logger.Information("ADDED To 3 Graphs DEVICE : {0} TemperatureDefault {1}, TemperatureMid {2}, TemperatureProcess {3}", ActiveDevice?.IPAddress,
				RoomInfo.Temperature, RoomInfo.TemperatureMiddle, RoomInfo.TemperatureProcess);

			GraphLineModelForDefault.GetFirst().Points.Add(new DataPoint(DateTimeAxis.ToDouble(RoomInfo.Date), RoomInfo.Temperature));
			GraphLineModelForMiddle.GetFirst().Points.Add(new DataPoint(DateTimeAxis.ToDouble(RoomInfo.Date), RoomInfo.TemperatureMiddle));
			GraphLineModelForProcess.GetFirst().Points.Add(new DataPoint(DateTimeAxis.ToDouble(RoomInfo.Date), RoomInfo.TemperatureProcess));
		}



		public override void SetInvalid(bool value)
		{
			if (value != IsInvalid)
			{
				IsInvalid = value;

			}
		}

		protected override void ReciveMessageOnSuccessEventHandler(MultiRoomInfo roomInfo)
		{
			RoomInfo = roomInfo;
		}

		private void LineSeriesVisibilityChange(string property, bool value)
		{
			if (GraphLineModelForDefault.Series != null)
			{
				if (property == nameof(TemperatureLineVisibilityForDefault))
				{
					var t = GraphLineModelForDefault.GetFirst();
					if (value != t.IsVisible)
					{
						t.IsVisible = value;
					}
				}
				if (property == nameof(HumidityLineVisibilityForDefault))
				{
					var h = GraphLineModelForDefault.GetLast();
					if (value != h.IsVisible)
					{
						h.IsVisible = value;
					}
				}
			}

			if (GraphLineModelForMiddle.Series != null)
			{
				if (property == nameof(TemperatureLineVisibilityForMiddle))
				{
					var t = GraphLineModelForMiddle.GetFirst();
					if (value != t.IsVisible)
					{
						t.IsVisible = value;
					}
				}
				if (property == nameof(HumidityLineVisibilityForMiddle))
				{
					var h = GraphLineModelForMiddle.GetLast();
					if (value != h.IsVisible)
					{
						h.IsVisible = value;
					}
				}
			}

			if (GraphLineModelForProcessForNord.Series != null)
			{
				if (property == nameof(TemperatureLineVisibilityForNord))
				{
					var t = GraphLineModelForProcessForNord.GetFirst();
					if (value != t.IsVisible)
					{
						t.IsVisible = value;
					}
				}
				if (property == nameof(HumidityLineVisibilityForNord))
				{
					var h = GraphLineModelForProcessForNord.GetLast();
					if (value != h.IsVisible)
					{
						h.IsVisible = value;
					}
				}
			}

			if (GraphLineModelForProcess.Series != null)
			{
				if (property == nameof(TemperatureLineVisibilityForProcess))
				{
					var t = GraphLineModelForProcess.GetFirst();
					if (value != t.IsVisible)
					{
						t.IsVisible = value;
					}
				}
				if (property == nameof(HumidityLineVisibilityForProcess))
				{
					var h = GraphLineModelForProcess.GetLast();
					if (value != h.IsVisible)
					{
						h.IsVisible = value;
					}
				}
			}
		}
	}
}
