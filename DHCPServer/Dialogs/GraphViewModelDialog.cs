using DHCPServer.Core.Extensions;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
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
		private bool _dispose = false;
		private IEnumerable<LineAnnotation> _annotations = new List<LineAnnotation>();
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

		private string _labelResult;
		public string LabelResult
		{
			get { return _labelResult; }
			set { SetProperty(ref _labelResult, value); }
		}

		private string _temperature ="Температура";
		public string Temperature
		{
			get { return _temperature; }
			set { SetProperty(ref _temperature, value); }
		}
		private string _humidity = "Влажность";
		public string Humidity
		{
			get { return _humidity; }
			set { SetProperty(ref _humidity, value); }
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
			LabelResult = string.Empty;
			if (!DateTimeSpan.IsTimeValidate()) 
			{
				LabelResult = "Ввод времени ошибочное";
				return;
			}
			IEnumerable<RoomInfo> collection;
			if (DateTimeSpan.IsTimeInclude)
			{
				collection = await _roomRepository.FilterRooms(GraphInfo.ActiveDevice.Id, DateTimeSpan.FromDate);
			}
			else
			{
				collection = await _roomRepository.FilterRooms(GraphInfo.ActiveDevice.Id, DateTimeSpan.FromDate, DateTimeSpan.FromTime, DateTimeSpan.ToTime);
			}

			if (collection.Count() > 0)
			{
				GraphInfo = FillModel(collection);
				_annotations = GraphInfo.GraphLineModel.Annotations.Where(x => x.Tag?.ToString() == "period").Cast<LineAnnotation>();
			}
			
			LabelResult = $"Найдено данных {collection.Count()} шт на {DateTimeSpan.FromDate:yyyy/MM/dd}";
		}

		public RoomLineGraphInfo FillModel(IEnumerable<RoomInfo> collection)
		{
			var result = new RoomLineGraphInfo(GraphInfo.ActiveDevice,false);

			var humidityLineSerie = result.GraphLineModel.GetLast();
			var temperatureLineSerie = result.GraphLineModel.GetFirst();
			var humidityPoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.Humidity)).ToList();
			var temperaturePoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.Temperature)).ToList();
			var min = collection.Min(x => x.Date);
		//	result.GraphLineModel.Axes[0].Minimum = DateTimeAxis.ToDouble(min);
		//	result.GraphLineModel.Axes[0].Maximum = DateTimeAxis.ToDouble(min.AddHours(6));
			result.GraphLineModel.FillCollection(temperatureLineSerie, temperaturePoints);
			result.GraphLineModel.FillCollection(humidityLineSerie, humidityPoints);

			result.GraphLineModel.SetLastNHours(6);
			result.GraphLineModel.AddAnnotationEveryDay();

			GraphInfo.GraphLineModel.InvalidatePlot(true);
			return result;
		}


		public override void OnDialogOpened(IDialogParameters parameters)
		{
			if (parameters != null)
			{
				bool setSetting = true;
				_current = parameters.GetValue<RoomLineGraphInfo>("model");
				GraphInfo = _current;

				var title = $"{_current?.ActiveDevice?.Nick} {_current?.ActiveDevice?.IPAddress}";
				Title = title;
				if(parameters.TryGetValue("date",out DateTimeSpanFilter date))
				{
					setSetting = false;
					Task.Run(async () =>
					{
						_dispose = true;
						   var collection = await _roomRepository.FilterRooms(_current.ActiveDevice.IPAddress, date.FromDate, date.ToDate);
						GraphInfo = FillModel(collection);
						_annotations = GraphInfo.GraphLineModel.Annotations.Where(x => x.Tag?.ToString() == "period").Cast<LineAnnotation>();

					});
				} 
				if(parameters.TryGetValue("dataType",out int dataType))
                {
					if(dataType==2) { Temperature += " мид";Humidity += " мид"; }
					if(dataType==3) { Temperature += " норд";Humidity += " норд"; }
					if(dataType==4) { Temperature += " процесс";Humidity += " процесс"; }

                }
				if(setSetting)
					SetSettings();

			}
		}

        

        private void SetSettings()
		{
			if (GraphInfo?.GraphLineModel == null) return;

			GraphInfo.GraphLineModel.Axes[0].Reset();
			GraphInfo.GraphLineModel.Axes[1].Reset();
			GraphInfo.GraphLineModel.SetLastNHours(6);
			GraphInfo.GraphLineModel.AddAnnotationEveryDay();
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

			var leftAxis = GraphInfo.GraphLineModel.Axes[1];
			var rightAxis = GraphInfo.GraphLineModel.Axes[0];
			if (_wheelCount == -2)
			{
				GraphInfo.GraphLineModel.ResetAllAxes();
				rightAxis.MajorStep = 1.0 / 24;
			}

			if(_wheelCount== -6)
			{
				rightAxis.MajorStep = 1.0 / 12;
			}

			if (_wheelCount == -8)
			{
				rightAxis.MajorStep = 1.0 / 6;
				_annotations.Select(x => { x.StrokeThickness = 2; return x; }); 
			}

			if(_wheelCount == -15)
			{
				_annotations.Select(x => { x.StrokeThickness = 1; return x; });
				rightAxis.MajorStep = 1.0 / 2;
				rightAxis.StringFormat = "HH:mm";
				((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Hours;
			}
			if (_wheelCount == -21)
			{
				rightAxis.MajorStep = 1.0;
				rightAxis.StringFormat = "dd/MM/yyy";
				((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Days;

			}

			if (_wheelCount == -25)
			{
				rightAxis.MajorStep = 1.5;
				rightAxis.StringFormat = "dd/MM/yyy";
				((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Days;

			}


			if (_wheelCount == 8)
			{
				leftAxis.MinimumMajorStep = 3;
			}

			if (_wheelCount == 9)
			{
				leftAxis.MinimumMajorStep = 2;
			}
			if (_wheelCount == 13)
			{

				leftAxis.MinimumMajorStep = 1;
			}
			if (_wheelCount == 17)
			{

				leftAxis.MinimumMajorStep = 0.5;
			}
			if (_wheelCount == 19)
			{

				leftAxis.MinimumMajorStep = 0.2;
			}
		}

		public override void OnDialogClosed()
		{
			if(_dispose)
				GraphInfo?.Dispose();
		}
	}
}
