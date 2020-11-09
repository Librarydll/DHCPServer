using DHCPServer.Domain.Models;
using DHCPServer.Models.Enums;
using OxyPlot;
using OxyPlot.Axes;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DHCPServer.Models.Infrastructure
{
	public class RoomLineGraphInfo : RoomInfo,IDisposable
	{
        public DeviceClient DeviceClient { get; set; }

		private RoomLineGraphInfoSetting _setting;
		public RoomLineGraphInfoSetting Setting
		{
			get { return _setting; }
			set { SetProperty(ref _setting, value); }
		}
		private CancellationTokenSource tokenSource = null;
		private bool _disposed = false;
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

		public RoomLineGraphInfo(RoomData roomData, ActiveDevice device) : base(roomData, device)
		{
			GraphLineModel = ViewResolvingPlotModel.CreateDefault();
			Setting = new RoomLineGraphInfoSetting();
			tokenSource = new CancellationTokenSource();
			DeviceClient = new DeviceClient(ActiveDevice);
			DeviceClient.ReciveMessageEvent += ReciveMessageEventHandler;
			DeviceClient.ReciveMessageErrorEvent += ReciveMessageOnErrorEventHandler;
			DeviceClient.EnableDeviceEvent += ReciveMessageOnValidEventHandler;
		}

		public RoomLineGraphInfo(ActiveDevice device) :this(new RoomData(),device)
		{}

		public async Task InitializeDeviceAsync()
		{		
			await DeviceClient.ListenAsync(tokenSource.Token);
		}

		public void CancelToken()
		{
			tokenSource.Cancel();
		}

        private void ReciveMessageEventHandler(RoomInfo roomInfo, DeviceResponseStatus status)
        {
			if (status == DeviceResponseStatus.Success)
			{
				Calculate(roomInfo.Temperature, roomInfo.Humidity);
			}
		}

        private void ReciveMessageOnErrorEventHandler(ActiveDevice device)
		{
			SetInvalid(true);
		}

		private void ReciveMessageOnValidEventHandler(ActiveDevice device)
		{
			SetInvalid(false);
		}


		public void SetSetting(double t,double h)
		{
			Setting.SetSetting(t, h);
		}
		public void SetInvalid(bool value)
		{
			if (value != IsInvalid)
			{
				IsInvalid = value;
				Temperature = 0;
				Humidity = 0;
				RaisePropertyChangedEvent("IsInvalid");
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
			
			Log.Logger.Information("ADDED To Graph DEVICE : {0} HUMIDITY {1}", ActiveDevice?.IPAddress, Humidity);
			GraphLineModel.GetLast().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), Humidity));
		}
		public void AddToTemperature()
		{
			Log.Logger.Information("ADDED To Graph DEVICE : {0} TEMPERATURE {1}", ActiveDevice?.IPAddress, Temperature);
			GraphLineModel.GetFirst().Points.Add(new DataPoint(DateTimeAxis.ToDouble(Date), Temperature));
		}


		public void Calculate(double t,double h)
		{
			var temp = t - Setting.TemperatureRange;
			var hum = h - Setting.HumidityRange;
			Temperature = temp;
			Humidity = hum;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				DeviceClient?.Dispose();
			}
		}
	}

}
