using DHCPServer.Core.Events;
using DHCPServer.Core.Events.Model;
using DHCPServer.Core.Extensions;
using DHCPServer.Dialogs.Extenstions;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models;
using DHCPServer.Models.Infrastructure;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace DHCPServer.ViewModels
{
	public class DeviceInformationViewModel : BindableBase
	{
		#region Fields
		private readonly IDialogService _dialogService;
		private readonly IRoomRepository _roomRepository;
		private readonly IActiveDeviceRepository _activeDeviceRepository;
		private readonly IReportRepository _reportRepository;
		private readonly ILogger _logger;
		private readonly IEventAggregator _eventAggregator;
		private readonly DispatcherTimer _timer;
		private bool _isActive = false;
		#endregion

		#region Commands
		public DelegateCommand OpenNewDevcieViewCommand { get; set; }
		public DelegateCommand<RoomLineGraphInfo> DeleteRoomCommand { get; set; }
		public DelegateCommand<RoomLineGraphInfo> OpenGraphCommand { get; set; }
		public DelegateCommand<RoomLineGraphInfo> OpenCalibrationCommand { get; set; }
		#endregion
		#region BindingProperties
		private ObservableCollection<RoomLineGraphInfo> _roomsCollection;
		public ObservableCollection<RoomLineGraphInfo> RoomsCollection
		{
			get { return _roomsCollection; }
			set { SetProperty(ref _roomsCollection, value); }
		}

		#endregion

		public DeviceInformationViewModel(IDialogService dialogService,
			IRoomRepository roomRepository,
			IActiveDeviceRepository activeDeviceRepository,
			IReportRepository reportRepository,
			ILogger logger,
			IEventAggregator eventAggregator)
		{
			OpenNewDevcieViewCommand = new DelegateCommand(OpenNewDevcieView);
			DeleteRoomCommand = new DelegateCommand<RoomLineGraphInfo>(DeleteRoom);
			OpenGraphCommand = new DelegateCommand<RoomLineGraphInfo>(OpenGraph);
			OpenCalibrationCommand = new DelegateCommand<RoomLineGraphInfo>(OpenCalibration);

			_dialogService = dialogService;
			_roomRepository = roomRepository;
			_activeDeviceRepository = activeDeviceRepository;
			_reportRepository = reportRepository;
			_logger = logger;
			_eventAggregator = eventAggregator;

			_timer = new DispatcherTimer
			{
				Interval = new TimeSpan(0, 1, 0)
			};
			_timer.Tick += _timer_Tick;
			_timer.Start();
			_eventAggregator.GetEvent<DeviceUpdateEvent>().Subscribe(DeviceUpdateEventHandler);
			Task.Run(async () => await InitializeAsync());
		}

		public async Task InitializeAsync()
		{
			try
			{
				var devices = await _activeDeviceRepository.GetActiveDevicesLists();
				var rooms = devices.Select(x => new RoomLineGraphInfo(x));
				RoomsCollection = new ObservableCollection<RoomLineGraphInfo>(rooms);
				_isActive = true && RoomsCollection.Count > 0;
				var tasks = new Task[RoomsCollection.Count];
				for (int i = 0; i < RoomsCollection.Count; i++)
				{
					tasks[i] = RoomsCollection[i].InitializeDeviceAsync();
				}
				await Task.WhenAll(tasks).ConfigureAwait(false);
			}
			catch (Exception ex)
			{

				_logger.Error("Не удлаось получить данные");
				_logger.Error("Ошибка {0}", ex?.Message);
				_logger.Error("Ошибка {0}", ex?.InnerException);
			}
		}

		private void DeviceUpdateEventHandler(DeviceEventModel device)
		{
			var d = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.IPAddress == device.OldValue.IPAddress);
			if (d != null)
			{
				d.ActiveDevice.Set(device.NewValue);
			}
		}

		private void DeleteRoom(RoomLineGraphInfo roomInfo)
		{
			RoomsCollection.Remove(roomInfo);
			roomInfo.ActiveDevice.IsAdded = false;
			_activeDeviceRepository.DeatachDevice(roomInfo.ActiveDevice).Wait();
			roomInfo.CancelToken();
			roomInfo.Dispose();
			_isActive = RoomsCollection.Count > 0;

		}

		private void _timer_Tick(object sender, EventArgs e)
		{
			if (!_isActive) return;

			foreach (var room in RoomsCollection)
			{
				room.AddToCollection();
			}
			Task.Run(async () =>
			  {


				  await _roomRepository.SaveAsync(RoomsCollection.Select(x=>x.RoomInfo));
				  _logger.Information("Данные успешно добавились в бд");


				  //var reports = await _reportRepository.TryCloseExpiredReports();

				  //if (reports.Count() > 0)
				  //{
					 // var rooms = RoomsCollection.DisposeRange(x => reports.Any(z => z.Id == x.ActiveDevice.ReportId));
					 // Application.Current.Dispatcher.Invoke(() =>
					 // {
						//  for (int i = 0; i < rooms.Count(); i++)
						//  {
						//	  RoomsCollection.RemoveAt(i);
						//  }
						//  _isActive = RoomsCollection.Count > 0;
					 // });
				  //}


			  }).ContinueWith(t =>
			  {

				  _logger.Error("Не удлаось добавить данные");
				  _logger.Error("Ошибка {0}", t.Exception?.Message);
				  _logger.Error("Ошибка {0}", t.Exception?.InnerException);

			  }, TaskContinuationOptions.OnlyOnFaulted);

		}


		//private void OpenNewDevcieView()
		//{
		//    ObservableCollection<ActiveDevice> devices = null;
		//    ICollection<ActiveDevice> inactiveDevices = new List<ActiveDevice>();
		//    ICollection<ActiveDevice> activeDevices = new List<ActiveDevice>();
		//    ICollection<RoomLineGraphInfo> rooms = new List<RoomLineGraphInfo>();

		//    _dialogService.ShowModal("SelectionDeviceView", x =>
		//    {
		//        if (x.Result == ButtonResult.OK)
		//        {
		//            devices = x.Parameters.GetValue<ObservableCollection<ActiveDevice>>("model");
		//        }
		//    });

		//    if (devices != null)
		//    {
		//        foreach (var device in devices)
		//        {
		//            var room = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.Id == device.Id);
		//            if (room != null)
		//            {
		//                if (!device.IsAdded)
		//                {
		//                    RoomsCollection.Remove(room);
		//                    inactiveDevices.Add(device);
		//                }
		//            }

		//            else
		//            {
		//                if (device.IsAdded)
		//                {
		//                    var newRomm = new RoomLineGraphInfo(new RoomData(), device);
		//                    activeDevices.Add(device);
		//                    rooms.Add(newRomm);
		//                }
		//            }



		//        }
		//        Task.Run(async () =>
		//        {
		//            await _activeDeviceRepository.DeatachDevices(inactiveDevices);
		//            var actives = await _activeDeviceRepository.CheckDevices(activeDevices);
		//            if (rooms.Count > 0)
		//            {

		//                var tasks = new Task[rooms.Count];
		//                for (int i = 0; i < rooms.Count; i++)
		//                {
		//                    tasks[i] = rooms.ElementAt(i).InitializeDeviceAsync();
		//                }

		//                Application.Current.Dispatcher.Invoke(() =>
		//                {
		//                    RoomsCollection.AddRange(rooms);
		//                    _isActive = true;
		//                });
		//                await Task.WhenAll(tasks).ConfigureAwait(false);
		//            }

		//        }).ContinueWith(t =>
		//        {

		//            _logger.Error(t.Exception.Message);
		//            _logger.Error(t.Exception?.InnerException?.Message);
		//        }, TaskContinuationOptions.OnlyOnFaulted);

		//    }


		//}

		private void OpenNewDevcieView()
		{
			ActiveDevice newDevice = null;

			_dialogService.ShowModal("SelectionDeviceViewOld", x =>
			{
				if (x.Result == ButtonResult.OK)
				{
					newDevice = x.Parameters.GetValue<ActiveDevice>("model");
				}
			});

			if (newDevice != null)
			{
				var room = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.IPAddress == newDevice.IPAddress);
				if (room == null)
				{
					var newRomm = new RoomLineGraphInfo(newDevice);
					RoomsCollection.Add(newRomm);

					Task.Run(async () =>
					{
						await _activeDeviceRepository.CheckDevice(newDevice);
						_logger.Information("Added new device to listen {0}",newDevice.IPAddress);
						await newRomm.InitializeDeviceAsync();

					}).ContinueWith(t =>
					{
					_logger.Error(t.Exception.Message);
					_logger.Error(t.Exception?.InnerException?.Message);
					}, TaskContinuationOptions.OnlyOnFaulted);

				}

			}


		}
		private void OpenGraph(RoomLineGraphInfo roomLineGraphInfo)
		{
			var dialogParametr = new DialogParameters
			{
				{ "model", roomLineGraphInfo }
			};

			_dialogService.Show("GraphView", dialogParametr, x =>
			{
			});
		}

		private void OpenCalibration(RoomLineGraphInfo roomLineGraphInfo)
		{
			var dialogParametr = new DialogParameters
			{
				{ "model", roomLineGraphInfo }
			};
			RoomLineGraphInfoSetting setting = null;

			_dialogService.ShowModal("CalibrationView", dialogParametr, x =>
			{
				if (x.Result == ButtonResult.OK)
				{
					setting = x.Parameters.GetValue<RoomLineGraphInfoSetting>("model");
				}
			});
			if (setting != null)
			{
				roomLineGraphInfo.SetSetting(setting.TemperatureRange, setting.HumidityRange);
			}
		}

	}

}