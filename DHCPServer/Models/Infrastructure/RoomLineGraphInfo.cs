using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure.Common;
using OxyPlot;
using OxyPlot.Axes;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

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

		public RoomLineGraphInfo(ActiveDevice device) : base(device)
		{
			GraphLineModel = ViewResolvingPlotModel.CreateDefault();
			Setting = new RoomLineGraphInfoSetting();

		}


		public void CancelToken()
		{
			_tokenSource.Cancel();
		}

        protected override void ReciveMessageEventHandler(RoomInfo roomInfo)
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


		public void AddToCollection()
		{
			//if (RoomInfo.Temperature < 0 || RoomInfo.Humidity < 0) 
			//{
			//	return;		
			//}
			RoomInfo.Date = DateTime.Now;
			AddToHumidity();
			AddToTemperature();
		}

		public void AddToHumidity()
		{		
			Log.Logger.Information("ADDED To Graph DEVICE : {0} HUMIDITY {1}", ActiveDevice?.IPAddress, RoomInfo.Humidity);
			GraphLineModel.GetLast().Points.Add(new DataPoint(DateTimeAxis.ToDouble(RoomInfo.Date), RoomInfo.Humidity));
		}
		public void AddToTemperature()
		{
			Log.Logger.Information("ADDED To Graph DEVICE : {0} TEMPERATURE {1}", ActiveDevice?.IPAddress, RoomInfo.Temperature);
			GraphLineModel.GetFirst().Points.Add(new DataPoint(DateTimeAxis.ToDouble(RoomInfo.Date), RoomInfo.Temperature));
		}


		public void Calculate(double t,double h)
		{
			var temp = t - Setting.TemperatureRange;
			var hum = h - Setting.HumidityRange;
			RoomInfo.Temperature = temp;
			RoomInfo.Humidity = hum;
		}
	
	}

}
