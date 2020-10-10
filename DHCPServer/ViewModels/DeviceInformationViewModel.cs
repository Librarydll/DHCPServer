using DHCPServer.Core.Events;
using DHCPServer.Core.Events.Model;
using DHCPServer.Core.Extensions;
using DHCPServer.Dialogs.Extenstions;
using DHCPServer.Models;
using DHCPServer.Models.Enums;
using DHCPServer.Models.Infrastructure;
using DHCPServer.Models.Repositories;
using DHCPServer.Services;
using DHCPServer.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
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
		private int timerTick = 0;
		private readonly IDialogService _dialogService;
		private readonly IRoomRepository _roomRepository;
		private readonly ILogger _logger;
		private readonly XmlDeviceProvider _xmlDeviceProvider;
		private readonly IEventAggregator _eventAggregator;
		private readonly DispatcherTimer _timer;
		private CancellationTokenSource tokenSource = null;
		private ICollection<DeviceClient> _deviceClients = new List<DeviceClient>();
		#endregion

		#region Commands
		public DelegateCommand OpenNewDevcieViewCommand { get; set; }
		public DelegateCommand<RoomLineGraphInfo> DeleteRoomCommand { get; set; }
		public DelegateCommand<RoomLineGraphInfo> OpenGraphCommand { get; set; }
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
			ILogger logger,
			XmlDeviceProvider xmlDeviceProvider,
			IEventAggregator eventAggregator)
		{
			OpenNewDevcieViewCommand = new DelegateCommand(OpenNewDevcieView);
			DeleteRoomCommand = new DelegateCommand<RoomLineGraphInfo>(DeleteRoom);
			OpenGraphCommand = new DelegateCommand<RoomLineGraphInfo>(OpenGraph);
			_dialogService = dialogService;
			_roomRepository = roomRepository;
			_logger = logger;
			_xmlDeviceProvider = xmlDeviceProvider;
			_eventAggregator = eventAggregator;
			var devices = _xmlDeviceProvider.GetDevices();
			tokenSource = new CancellationTokenSource();
			RoomsCollection = new ObservableCollection<RoomLineGraphInfo>(_xmlDeviceProvider.CastDevices(devices).ToRoomLineGraphInfo());
			_timer = new DispatcherTimer
			{
				Interval = new TimeSpan(0, 10, 0)
			};
			_timer.Tick += _timer_Tick;
			_timer.Start();
			_deviceClients = new List<DeviceClient>(RoomsCollection
				.Select(x => new Device { IPAddress = x.IPAddress })
				.ToDeviceClient());
			Task.Run(async () => await StartListenAsync());
			_eventAggregator.GetEvent<DeviceUpdateEvent>().Subscribe(DeviceUpdateEventHandler);
		}

		private void DeviceUpdateEventHandler(DeviceEventModel device)
		{
			var dc = _deviceClients.FirstOrDefault(x => x.Device.IPAddress == device.OldValue.IPAddress);

			if (dc != null)
			{
				dc.Device = device.NewValue;
			}

			var d = RoomsCollection.FirstOrDefault(x => x.Device.IPAddress == device.OldValue.IPAddress);
			if (d != null)
			{
				d.Device = device.NewValue;
			}
			_xmlDeviceProvider.SaveDevices(RoomsCollection.Select(x => new Device { IPAddress = x.IPAddress, Nick = x.Device.Nick }));
		}

		private async Task StartListenAsync()
		{
			foreach (var device in _deviceClients)
			{
				await SubscribeDeviceAsync(device);
			}
		}

		private void DeleteRoom(RoomLineGraphInfo roomInfo)
		{
			var d = _deviceClients.FirstOrDefault(x => x.Device.IPAddress == roomInfo.Device.IPAddress);
			d?.Dispose();
			RoomsCollection.Remove(roomInfo);
			_xmlDeviceProvider.SaveDevices(RoomsCollection.Select(x => new Device { IPAddress = x.IPAddress }));
		}

		private void _clientService_ReciveMessageErrorEvent(Device device)
		{
			var invalidDevide = RoomsCollection.FirstOrDefault(x => x.IPAddress == device.IPAddress);
			Application.Current.Dispatcher.Invoke(new Action(() => { invalidDevide.SetInvalid(true); }));

		}
		private void _clientService_ReciveMessageEvent(RoomInfo roomInfo, DeviceResponseStatus status)
		{
			if (status == DeviceResponseStatus.Success)
			{
				var old = RoomsCollection.FirstOrDefault(x => x.IPAddress == roomInfo.IPAddress);
				Application.Current.Dispatcher.Invoke(() =>
				{
					if (old != null)
					{
						old.Humidity = roomInfo.Humidity;
						old.Temperature = roomInfo.Temperature;

					}
				});

			}
		}

		private void _timer_Tick(object sender, EventArgs e)
		{
			timerTick += 1;
			if (timerTick >= 6)
			{
				foreach (var room in RoomsCollection)
				{
					room.AddToCollections();
				}
				timerTick = 0;
			}

			foreach (var room in RoomsCollection)
			{
				if (room.Humidity < 0)
					room.Humidity = room.OldPositiveHumidityValue;
				if (room.Temperature < 0)
					room.Temperature = room.OldPositiveTemperatureValue;
				room.Date = DateTime.Now;
			}


			Task.Run(async () =>
			{
				
				await _roomRepository.SaveAsync(RoomsCollection);
				_logger.Information("Не удлаось добавить данные");

			}).ContinueWith(t=> {

				_logger.Error("Не удлаось добавить данные");
				_logger.Error("Ошибка {0}", t.Exception?.Message);
				_logger.Error("Ошибка {0}", t.Exception?.InnerException);

			},TaskContinuationOptions.OnlyOnFaulted);

		}


		private async Task SubscribeDeviceAsync(DeviceClient device)
		{
			device.ReciveMessageEvent += _clientService_ReciveMessageEvent;
			device.ReciveMessageErrorEvent += _clientService_ReciveMessageErrorEvent;
			device.EnableDeviceEvent += d =>
			{
				var invalidDevide = RoomsCollection.FirstOrDefault(x => x.IPAddress == d.IPAddress);
				Application.Current.Dispatcher.Invoke(new Action(() => { invalidDevide.SetInvalid(false); }));
			};
			await device.ListenAsync(tokenSource.Token);
		}

		private void OpenNewDevcieView()
		{
			Device newDevice = null;

			_dialogService.ShowModal("SelectionDeviceView", x =>
			{
				if (x.Result == ButtonResult.OK)
				{
					newDevice = x.Parameters.GetValue<Device>("model");
				}
			});

			if (newDevice != null)
			{
				var room = RoomsCollection.FirstOrDefault(x => x.IPAddress == newDevice.IPAddress);
				if (room == null)
				{
					var newRomm = new RoomLineGraphInfo(new RoomData(), newDevice);
					RoomsCollection.Add(newRomm);
					var deviceClient = new DeviceClient(newDevice);
					_deviceClients.Add(deviceClient);
					Task.Run(async () => await SubscribeDeviceAsync(deviceClient));
					_xmlDeviceProvider.SaveDevices(RoomsCollection.Select(x => new Device { IPAddress = x.IPAddress,Nick=x.Device.Nick }));

				}

			}
		}
		private void OpenGraph(RoomLineGraphInfo roomLineGraphInfo)
		{
			var dialogParametr = new DialogParameters
			{
				{ "model", roomLineGraphInfo }
			};

			_dialogService.ShowModal("GraphView", dialogParametr, x =>
			{
				
			});		
		}

	}
}
