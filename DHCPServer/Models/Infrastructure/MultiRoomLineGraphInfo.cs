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
		private Visibility _roomLineModelForProcess =Visibility.Visible;
		public Visibility RoomLineModelForProcess
		{
			get { return _roomLineModelForProcess; }
			set { SetProperty(ref _roomLineModelForProcess, value); }
		}
		private Visibility _roomLineModelForDefault = Visibility.Visible;
		public Visibility RoomLineModelForDefault
		{
			get { return _roomLineModelForDefault; }
			set { SetProperty(ref _roomLineModelForDefault, value); }
		}
		private Visibility _roomLineModelForMiddle = Visibility.Visible;
		public Visibility RoomLineModelForMiddle
		{
			get { return _roomLineModelForMiddle; }
			set { SetProperty(ref _roomLineModelForMiddle, value); }
		}

		private Visibility _roomLineModelForNord = Visibility.Visible;
		public Visibility RoomLineModelForNord
		{
			get { return _roomLineModelForNord; }
			set { SetProperty(ref _roomLineModelForNord, value); }
		}

		public MultiRoomLineGraphInfo(ActiveDevice device) : base(device)
		{
			GraphLineModelForDefault = ViewResolvingPlotModel.CreateDefault();
			GraphLineModelForMiddle = ViewResolvingPlotModel.CreateDefault();
			GraphLineModelForProcess = ViewResolvingPlotModel.CreateDefault();

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
	}
}
