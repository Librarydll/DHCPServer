﻿using DHCPServer.Core.Extensions;
using DHCPServer.Models;
using DHCPServer.Models.Infrastructure;
using DHCPServer.Models.Repositories;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DHCPServer.Dialogs
{
	public class GraphViewModelDialog : DialogViewModelBase
	{
		private int _wheelCount = 0;
		private readonly IRoomRepository _roomRepository;

		private RoomLineGraphInfo _current;

		private DateTimeSpanFilter _dateTimeSpan = new DateTimeSpanFilter();

		public DateTimeSpanFilter DateTimeSpan
		{
			get { return _dateTimeSpan; }
			set { SetProperty(ref _dateTimeSpan, value); }
		}



		private RoomLineGraphInfo _graphInfo;

		public RoomLineGraphInfo GraphInfo
		{
			get { return _graphInfo; }
			set { SetProperty(ref _graphInfo, value); }
		}

		private string _result;
		public string Result
		{
			get { return _result; }
			set { SetProperty(ref _result, value); }
		}

		public DelegateCommand FilterCommand { get; set; }
		public DelegateCommand ShowRealTimeGraphComamand { get; set; }

		public GraphViewModelDialog(IRoomRepository roomRepository)
		{
			FilterCommand = new DelegateCommand(async () => await FilterHandler());
			ShowRealTimeGraphComamand = new DelegateCommand(ShowRealTimeGraphHandler);
			_roomRepository = roomRepository;
		}

		private void ShowRealTimeGraphHandler()
		{
			GraphInfo = _current;
		}

		private async Task FilterHandler()
		{
			Result = string.Empty;
			if (!DateTimeSpan.IsTimeValidate()) 
			{
				Result = "Ввод времени ошибочное";
				return;
			}
			IEnumerable<RoomInfo> collection = null;
			if (DateTimeSpan.IsTimeInclude)
			{
				collection = await _roomRepository.FilterRooms(GraphInfo.DeviceId, DateTimeSpan.FromDate);
			}
			else
			{
				collection = await _roomRepository.FilterRooms(GraphInfo.DeviceId, DateTimeSpan.FromDate, DateTimeSpan.FromTime, DateTimeSpan.ToTime);
			}

			if (collection.Count() > 0)
			{
				GraphInfo = FillModel(collection);
				GraphInfo.GraphLineModel.InvalidatePlot(true);
			}
			
			Result = $"Найдено данных {collection.Count()} шт на {DateTimeSpan.FromDate.ToString("yyyy/MM/dd")}";
		}

		public RoomLineGraphInfo FillModel(IEnumerable<RoomInfo> collection)
		{
			var result = new RoomLineGraphInfo(new RoomData(), GraphInfo.Device);

			var humidityLineSerie = result.GraphLineModel.GetLast();
			var temperatureLineSerie = result.GraphLineModel.GetFirst();

			var humidityPoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date.ToToday()), x.Humidity)).ToList();
			var temperaturePoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date.ToToday()), x.Temperature)).ToList();

			temperatureLineSerie.Points.AddRange(temperaturePoints);
			humidityLineSerie.Points.AddRange(humidityPoints);
			GraphInfo.GraphLineModel.InvalidatePlot(true);
			return result;
		}


		public override void OnDialogOpened(IDialogParameters parameters)
		{
			if (parameters != null)
			{
				_current = parameters.GetValue<RoomLineGraphInfo>("model");
				GraphInfo = _current;
				GraphInfo.GraphLineModel.Axes[0].Reset();
				GraphInfo.GraphLineModel.Axes[1].Reset();
			}
		}

		public void LineMouseWheelEventHandler(MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
				_wheelCount--;
			}
			if (e.Delta >= 0)
			{
				_wheelCount++;
			}

			var axis = GraphInfo.GraphLineModel.Axes[1];
			if (_wheelCount == 8)
			{
				axis.MinimumMajorStep = 3;
			}

			if (_wheelCount == 9)
			{
				axis.MinimumMajorStep = 2;
			}
			if (_wheelCount == 13)
			{

				axis.MinimumMajorStep = 1;
			}
			if (_wheelCount == 17)
			{

				axis.MinimumMajorStep = 0.5;
			}
			if (_wheelCount == 19)
			{

				axis.MinimumMajorStep = 0.2;
			}
		}

	}
}