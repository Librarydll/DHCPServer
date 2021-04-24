using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure.Common;
using OxyPlot;
using OxyPlot.Axes;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DHCPServer.Models.Infrastructure
{
	public class RoomLineGraphInfo : RoomLineBase<ActiveDevice,RoomInfo>
	{
		private RoomLineGraphInfoSetting _setting;
		public RoomLineGraphInfoSetting Setting
		{
			get { return _setting; }
			set { SetProperty(ref _setting, value); }
		}

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

		public static RoomLineGraphInfo CreateDefault() => new RoomLineGraphInfo(new ActiveDevice(new Device("1", "1")));

		public RoomLineGraphInfo(ActiveDevice device,bool startTimer=true) : base(device)
		{
			GraphLineModel = ViewResolvingPlotModel.CreateDefault();
			Setting = new RoomLineGraphInfoSetting();
			if(startTimer)
				_timer = new Timer(_timer_Tick,null,new TimeSpan(0,10,0),new TimeSpan(0,10,0));
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

	
		public void CancelToken()
		{
			_tokenSource.Cancel();
		}

        protected override void ReciveMessageOnSuccessEventHandler(RoomInfo roomInfo)
        {
			Calculate(roomInfo.Temperature, roomInfo.Humidity);
		}

		public void SetSetting(double t,double h)
		{
			Setting.SetSetting(t, h);
		}
		public override void SetInvalid(bool value)
		{
			if (value != IsInvalid)
			{
				IsInvalid = value;
                RoomInfo.Temperature = 0;
				RoomInfo.Humidity = 0;
            }
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
			Log.Logger.Information("ADDED To Graph DEVICE : {0} HUMIDITY {1}", ActiveDevice?.IPAddress, RoomInfo.Humidity);
			GraphLineModel.AddDataPoint(GraphLineModel.GetLast(), new DataPoint(DateTimeAxis.ToDouble(RoomInfo.Date), RoomInfo.Humidity));
		}
		public void AddToTemperature()
		{
			Log.Logger.Information("ADDED To Graph DEVICE : {0} TEMPERATURE {1}", ActiveDevice?.IPAddress, RoomInfo.Temperature);
			GraphLineModel.AddDataPoint(GraphLineModel.GetFirst(), new DataPoint(DateTimeAxis.ToDouble(RoomInfo.Date), RoomInfo.Temperature));
		}


		public void Calculate(double t,double h)
		{
			var temp = t + Setting.TemperatureRange;
			var hum = h + Setting.HumidityRange;
			RoomInfo.Temperature = temp;
			RoomInfo.Humidity = hum;
		}
	
	}

}
