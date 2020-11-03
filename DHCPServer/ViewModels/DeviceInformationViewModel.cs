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
		private readonly ILogger _logger;
		private readonly IEventAggregator _eventAggregator;
		private readonly IDeviceRepository _deviceRepository;
		private readonly DispatcherTimer _timer;
		private CancellationTokenSource tokenSource = null;
		private ICollection<DeviceClient> _deviceClients = new List<DeviceClient>();
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
			ILogger logger,
			IEventAggregator eventAggregator,
			IDeviceRepository deviceRepository, Transfer transfer)
		{
			OpenNewDevcieViewCommand = new DelegateCommand(OpenNewDevcieView);
			DeleteRoomCommand = new DelegateCommand<RoomLineGraphInfo>(DeleteRoom);
			OpenGraphCommand = new DelegateCommand<RoomLineGraphInfo>(OpenGraph);
			OpenCalibrationCommand = new DelegateCommand<RoomLineGraphInfo>(OpenCalibration);

			_dialogService = dialogService;
			_roomRepository = roomRepository;
			_activeDeviceRepository = activeDeviceRepository;
			_logger = logger;
			_eventAggregator = eventAggregator;
			_deviceRepository = deviceRepository;
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
			var dc = _deviceClients.FirstOrDefault(x => x.ActiveDevice.Device.IPAddress == device.OldValue.IPAddress);

			if (dc != null)
			{
				dc.ActiveDevice.Device = device.NewValue;
			}

			var d = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.Device.IPAddress == device.OldValue.IPAddress);
			if (d != null)
			{
				d.ActiveDevice.Device = device.NewValue;
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
			var d = _deviceClients.FirstOrDefault(x => x.ActiveDevice.Device.IPAddress == roomInfo.ActiveDevice.Device.IPAddress);
			d?.Dispose();
			RoomsCollection.Remove(roomInfo);
			roomInfo.ActiveDevice.IsAdded = false;

			_activeDeviceRepository.DeatachDevice(roomInfo.ActiveDevice).Wait();
		}

		private void ClientService_ReciveMessageErrorEvent(ActiveDevice device)
		{
			var invalidDevide = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.Device.IPAddress == device.Device.IPAddress);

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
				var old = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.Device.IPAddress == roomInfo.ActiveDevice.Device.IPAddress);
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
				var invalidDevide = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.Device.IPAddress == d.Device.IPAddress);
				if (invalidDevide != null)
					Application.Current.Dispatcher.Invoke(new Action(() => { invalidDevide.SetInvalid(false); }));
			};
			await device.ListenAsync(tokenSource.Token);
		}

		private void OpenNewDevcieView()
		{
			ObservableCollection<Device> devices = null;
			ICollection<ActiveDevice> inactiveDevices = new List<ActiveDevice>();
			ICollection<ActiveDevice> activeDevices = new List<ActiveDevice>();
			ICollection<RoomLineGraphInfo> rooms = new List<RoomLineGraphInfo>();

			_dialogService.ShowModal("SelectionDeviceView", x =>
			{
				if (x.Result == ButtonResult.OK)
				{
					devices = x.Parameters.GetValue<ObservableCollection<Device>>("model");
				}
			});

			if (devices != null)
			{
				foreach (var device in devices)
				{
					var room = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.Id == device.ActiveDevice.Id);
					device.ActiveDevice.DeviceId = device.Id;
					if (room != null) {					
						if (!device.ActiveDevice.IsAdded)
						{
							RoomsCollection.Remove(room);
							inactiveDevices.Add(device.ActiveDevice);
						}
					}

					else
					{

						if (device.ActiveDevice.IsAdded)
						{
							var newRomm = new RoomLineGraphInfo(new RoomData(), device.ActiveDevice);
							var deviceClient = new DeviceClient(device);
							_deviceClients.Add(deviceClient);
							device.ActiveDevice.Device = device;
							activeDevices.Add(device.ActiveDevice);
							rooms.Add(newRomm);

						}
					}

					

				}
				Task.Run(async () =>
				{
					await _activeDeviceRepository.DeatachDevices(inactiveDevices);
					var actives = await _activeDeviceRepository.CheckDevices(activeDevices);

					Application.Current.Dispatcher.Invoke(() =>
					{
						RoomsCollection.AddRange(rooms);
					});

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