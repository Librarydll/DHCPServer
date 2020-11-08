using DHCPServer.Core.Events;
using DHCPServer.Core.Events.Model;
using DHCPServer.Core.Extensions;
using DHCPServer.Dialogs.Extenstions;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models;
using DHCPServer.Models.Enums;
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
		private CancellationTokenSource tokenSource = null;
		private ICollection<DeviceClient> _deviceClients = new List<DeviceClient>();
		private bool _isActive =false;
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
			tokenSource = new CancellationTokenSource();

			_timer = new DispatcherTimer
			{
				Interval = new TimeSpan(0, 10, 0)
			};
			_timer.Tick += _timer_Tick;
			_timer.Start();



			Task.Run(async () =>
			{
				//await	transfer.TransferData();

				var devices = await _activeDeviceRepository.GetActiveDevicesLists();
				var rooms = devices.Select(x => new RoomLineGraphInfo(x));
				RoomsCollection = new ObservableCollection<RoomLineGraphInfo>(rooms);
				var activeDevices = RoomsCollection.Select(x => x.ActiveDevice);
				_deviceClients = new List<DeviceClient>(activeDevices.ToDeviceClient());
				_isActive = true ? RoomsCollection.Count > 0 : false;
				await StartListenAsync();

			}).ContinueWith(t=> {
				_logger.Error("Не удлаось получить данные");
				_logger.Error("Ошибка {0}", t.Exception?.Message);
				_logger.Error("Ошибка {0}", t.Exception?.InnerException);
			}
			,TaskContinuationOptions.OnlyOnFaulted);


			_eventAggregator.GetEvent<DeviceUpdateEvent>().Subscribe(DeviceUpdateEventHandler);
		}


		private void DeviceUpdateEventHandler(DeviceEventModel device)
		{
			var dc = _deviceClients.FirstOrDefault(x => x.ActiveDevice.IPAddress == device.OldValue.IPAddress);

			if (dc != null)
			{
				dc.ActiveDevice.Set(device.NewValue);
			}

			var d = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.IPAddress == device.OldValue.IPAddress);
			if (d != null)
			{
				d.ActiveDevice.Set(device.NewValue);
			}
		}

		private async Task StartListenAsync()
		{
			var tasks = new List<Task>();
			foreach (var device in _deviceClients)
			{
				tasks.Add(SubscribeDeviceAsync(device));
			}

			await Task.WhenAll(tasks);
		}

		private void DeleteRoom(RoomLineGraphInfo roomInfo)
		{
			var d = _deviceClients.FirstOrDefault(x => x.ActiveDevice.IPAddress == roomInfo.ActiveDevice.IPAddress);
			d?.Dispose();
			RoomsCollection.Remove(roomInfo);
			roomInfo.ActiveDevice.IsAdded = false;

			_activeDeviceRepository.DeatachDevice(roomInfo.ActiveDevice).Wait();
		}

		private void ClientService_ReciveMessageErrorEvent(ActiveDevice device)
		{
			var invalidDevide = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.IPAddress == device.IPAddress);

			if (invalidDevide != null)
				Application.Current.Dispatcher.Invoke(new Action(() =>
				{
					invalidDevide.SetInvalid(true);

				}));

		}
		private void _clientService_ReciveMessageEvent(RoomInfo roomInfo, DeviceResponseStatus status)
		{
			if (status == DeviceResponseStatus.Success)
			{
				var old = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.IPAddress == roomInfo.ActiveDevice.IPAddress);
				Application.Current.Dispatcher.Invoke(() =>
				{
					if (old != null)
					{
						old.Calculate(roomInfo.Temperature, roomInfo.Humidity);
					}
				});

			}
		}

		private void _timer_Tick(object sender, EventArgs e)
		{
			if (!_isActive) return;
			foreach (var room in RoomsCollection.Where(x => !x.IsAddedToGraph))
			{
				room.AddToCollection();
			}
			if (RoomsCollection.Any(x => !x.IsAddedToGraph))
			{
				_timer.Interval = new TimeSpan(0, 0, 5);
			}
			else
			{


				Task.Run(async () =>
				{
					var first = RoomsCollection.FirstOrDefault();
					if(await _reportRepository.TryCloseReport(first.ActiveDevice))
                    {
						_deviceClients.DisposeRange();
						_isActive = false;
					}


					await _roomRepository.SaveAsync(RoomsCollection.Where(x => x.IsAddedToGraph));
					_logger.Information("Данные успешно добавились в бд");

					foreach (var room in RoomsCollection)
					{
						room.IsAddedToGraph = false;
					}

					_timer.Interval = new TimeSpan(0, 10, 0);

				}).ContinueWith(t =>
				{

					_logger.Error("Не удлаось добавить данные");
					_logger.Error("Ошибка {0}", t.Exception?.Message);
					_logger.Error("Ошибка {0}", t.Exception?.InnerException);

				}, TaskContinuationOptions.OnlyOnFaulted);
			}

		}


		private async Task SubscribeDeviceAsync(DeviceClient device)
		{
			device.ReciveMessageEvent += _clientService_ReciveMessageEvent;
			device.ReciveMessageErrorEvent += ClientService_ReciveMessageErrorEvent;
			device.EnableDeviceEvent += d =>
			{
				var invalidDevide = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.IPAddress == d.IPAddress);
				if (invalidDevide != null)
					Application.Current.Dispatcher.Invoke(new Action(() => { invalidDevide.SetInvalid(false); }));
			};
			await device.ListenAsync(tokenSource.Token);
		}

		private void OpenNewDevcieView()
		{
			ObservableCollection<ActiveDevice> devices = null;
			ICollection<ActiveDevice> inactiveDevices = new List<ActiveDevice>();
			ICollection<ActiveDevice> activeDevices = new List<ActiveDevice>();
			ICollection<RoomLineGraphInfo> rooms = new List<RoomLineGraphInfo>();

			_dialogService.ShowModal("SelectionDeviceView", x =>
			{
				if (x.Result == ButtonResult.OK)
				{
					devices = x.Parameters.GetValue<ObservableCollection<ActiveDevice>>("model");
				}
			});

			if (devices != null)
			{
				foreach (var device in devices)
				{
					var room = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.IPAddress == device.IPAddress);
					if (room != null) {					
						if (!device.IsAdded)
						{
							RoomsCollection.Remove(room);
							inactiveDevices.Add(device);
						}
					}

					else
					{

						if (device.IsAdded)
						{
							var newRomm = new RoomLineGraphInfo(new RoomData(), device);
							var deviceClient = new DeviceClient(device);
							_deviceClients.Add(deviceClient);
							activeDevices.Add(device);
							rooms.Add(newRomm);

						}
					}

					

				}
				Task.Run(async () =>
				{
					await _activeDeviceRepository.DeatachDevices(inactiveDevices);
					var actives = await _activeDeviceRepository.CheckDevices(activeDevices);

                    if (rooms.Count > 0)
                    {
						Application.Current.Dispatcher.Invoke(() =>
						{
							RoomsCollection.AddRange(rooms);
							_isActive = true;
						});
					}
					

				}).ContinueWith(t =>
				{

					_logger.Error(t.Exception.Message);
					_logger.Error(t.Exception?.InnerException?.Message);
				}, TaskContinuationOptions.OnlyOnFaulted);

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
				roomLineGraphInfo.Setting.TemperatureRange = setting.TemperatureRange;
				roomLineGraphInfo.Setting.HumidityRange = setting.HumidityRange;
			}
		}


	}

}