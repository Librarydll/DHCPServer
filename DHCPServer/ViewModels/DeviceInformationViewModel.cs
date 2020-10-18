﻿using DHCPServer.Core.Events;
using DHCPServer.Core.Events.Model;
using DHCPServer.Core.Extensions;
using DHCPServer.Dialogs.Extenstions;
using DHCPServer.Models;
using DHCPServer.Models.Enums;
using DHCPServer.Models.Infrastructure;
using DHCPServer.Models.Repositories;
using DHCPServer.Services;
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
			IEventAggregator eventAggregator,
			IDeviceRepository deviceRepository,Transfer transfer)
		{
			OpenNewDevcieViewCommand = new DelegateCommand(OpenNewDevcieView);
			DeleteRoomCommand = new DelegateCommand<RoomLineGraphInfo>(DeleteRoom);
			OpenGraphCommand = new DelegateCommand<RoomLineGraphInfo>(OpenGraph);

			_dialogService = dialogService;
			_roomRepository = roomRepository;
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
			   var devices = await _deviceRepository.GetDevicesLists();
			   var rooms = devices.Select(x => new RoomLineGraphInfo(x));
			   RoomsCollection = new ObservableCollection<RoomLineGraphInfo>(rooms);

			   _deviceClients = new List<DeviceClient>(RoomsCollection
			.Select(x => new Device { IPAddress = x.Device.IPAddress })
			.ToDeviceClient());

			   await StartListenAsync();
		   });


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
		}

		private async Task StartListenAsync()
		{
			Task[] tasks = new Task[_deviceClients.Count];
			foreach (var device in _deviceClients)
			{
				tasks.Append(SubscribeDeviceAsync(device));
			}

			await Task.WhenAll(tasks);
		}

		private void DeleteRoom(RoomLineGraphInfo roomInfo)
		{
			var d = _deviceClients.FirstOrDefault(x => x.Device.IPAddress == roomInfo.Device.IPAddress);
			d?.Dispose();
			RoomsCollection.Remove(roomInfo);
			_deviceRepository.DeleteDeviceListAsync(roomInfo.Device.Id);
		}

		private void ClientService_ReciveMessageErrorEvent(Device device)
		{
			var invalidDevide = RoomsCollection.FirstOrDefault(x => x.Device.IPAddress == device.IPAddress);
			Application.Current.Dispatcher.Invoke(new Action(() => { invalidDevide.SetInvalid(true); }));

		}
		private void _clientService_ReciveMessageEvent(RoomInfo roomInfo, DeviceResponseStatus status)
		{
			if (status == DeviceResponseStatus.Success)
			{
				var old = RoomsCollection.FirstOrDefault(x => x.Device.IPAddress == roomInfo.Device.IPAddress);
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
				foreach (var room in RoomsCollection)
				{
					room.IsAddedToGraph = false;
				}

				_timer.Interval = new TimeSpan(0, 10, 0);

				Task.Run(async () =>
				{

					await _roomRepository.SaveAsync(RoomsCollection.Where(x => x.IsAddedToGraph));
					_logger.Information("Данные успешно добавились в бд");

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
				var invalidDevide = RoomsCollection.FirstOrDefault(x => x.Device.IPAddress == d.IPAddress);
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
				var room = RoomsCollection.FirstOrDefault(x => x.Device.IPAddress == newDevice.IPAddress);
				if (room == null)
				{
					var newRomm = new RoomLineGraphInfo(new RoomData(), newDevice);
					RoomsCollection.Add(newRomm);
					var deviceClient = new DeviceClient(newDevice);
					_deviceClients.Add(deviceClient);
					Task.Run(async () => await SubscribeDeviceAsync(deviceClient));
					_deviceRepository.CreateDeviceListAsync(new DevicesList() { DeviceId = newDevice.Id }).Wait();

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

	}

}