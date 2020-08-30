using DHCPServer.Core;
using DHCPServer.Core.Extensions;
using DHCPServer.Core.Graph;
using DHCPServer.Dialogs.Extenstions;
using DHCPServer.Models;
using DHCPServer.Models.Context;
using DHCPServer.Models.Infrastructure;
using DHCPServer.Models.Repositories;
using DHCPServer.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DHCPServer.ViewModels
{
	public class MainViewModel : BindableBase
	{
		#region Fields
		private bool _isTimerRunning = false;
		private readonly IDialogService _dialogService;
		private readonly IClientService _clientService;
		private readonly IRoomRepository _roomRepository;
		private readonly XmlDeviceProvider _xmlDeviceProvider;
		private readonly DispatcherTimer _timer;
		private CancellationTokenSource tokenSource = null;
		private ICollection<DeviceClient> _deviceClients = new List<DeviceClient>();
		#endregion

		#region Commands
		public DelegateCommand OpenNewDevcieViewCommand { get; set; }
		public DelegateCommand<RoomLineGraphInfo> DeleteRoomCommand { get; set; }
		#endregion
		#region BindingProperties
		private ObservableCollection<RoomLineGraphInfo> _roomsCollection;
		public ObservableCollection<RoomLineGraphInfo> RoomsCollection
		{
			get { return _roomsCollection; }
			set { SetProperty(ref _roomsCollection, value); }
		}

		private LineGraphProvider _lineGraph;
		public LineGraphProvider LineGraph
		{
			get { return _lineGraph; }
			set { SetProperty(ref _lineGraph, value); }
		}
		#endregion
		public MainViewModel(IDialogService dialogService, IClientService clientService,IRoomRepository roomRepository ,XmlDeviceProvider xmlDeviceProvider, LineGraphProvider lineGraph)
		{
			OpenNewDevcieViewCommand = new DelegateCommand(OpenNewDevcieView);
			DeleteRoomCommand = new DelegateCommand<RoomLineGraphInfo>(DeleteRoom);
			_dialogService = dialogService;
			_clientService = clientService;
			_roomRepository = roomRepository;
			_xmlDeviceProvider = xmlDeviceProvider;
			_lineGraph = lineGraph;
			var devices = _xmlDeviceProvider.GetDevices();
			tokenSource = new CancellationTokenSource();
			RoomsCollection = new ObservableCollection<RoomLineGraphInfo>(_xmlDeviceProvider.CastDevices(devices).ToRoomLineGraphInfo());
			_timer = new DispatcherTimer();
			_timer.Interval = new TimeSpan(1, 0, 0);
			_timer.Tick += _timer_Tick;
			_timer.Start();
			_clientService.ReciveMessageEvent += _clientService_ReciveMessageEvent;
			_clientService.ReciveMessageErrorEvent += _clientService_ReciveMessageErrorEvent;
			_deviceClients = new List<DeviceClient>(RoomsCollection
				.Select(x => new Device { IPAddress = x.IPAddress })
				.ToDeviceClient());
			Task.Run( async() =>await StartListenAsync());
			
			//	Task.Run(async () => { await _clientService.TryRecieve(tokenSource.Token, RoomsCollection.Select(x => new Device { IPAddress = x.IPAddress })); });
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
			RoomsCollection.Remove(roomInfo);
			_xmlDeviceProvider.SaveDevices(RoomsCollection.Select(x => new Device { IPAddress = x.IPAddress }));
		}

		private void _clientService_ReciveMessageErrorEvent(Device device)
		{
			var invalidDevide = RoomsCollection.FirstOrDefault(x => x.IPAddress == device.IPAddress);
			Application.Current.Dispatcher.Invoke(new Action(() => { RoomsCollection.Remove(invalidDevide); }));

		}
		private void _clientService_ReciveMessageEvent(RoomInfo roomInfo, DeviceResponseStatus status)
		{
			if (status == DeviceResponseStatus.Success)
			{
				var old = RoomsCollection.FirstOrDefault(x => x.IPAddress == roomInfo.IPAddress);
				if (old != null)
				{
					old.Humidity = roomInfo.Humidity;
					old.Temperature = roomInfo.Temperature;
				}
			}
		}

		private void _timer_Tick(object sender, EventArgs e)
		{
			_isTimerRunning = true;
			foreach (var room in RoomsCollection)
			{
				room.AddToCollections();
			}

			Task.Run(async () =>
			{
				await _roomRepository.SaveAsync(RoomsCollection);
			});


			_isTimerRunning = false;
		}


		private async Task SubscribeDeviceAsync(DeviceClient device)
		{
			device.ReciveMessageEvent += _clientService_ReciveMessageEvent;
			device.ReciveMessageErrorEvent += _clientService_ReciveMessageErrorEvent;
			await device.ListenAsync(tokenSource.Token);
		}

		private void OpenNewDevcieView()
		{
			Device newDevice = null;
			var dialogParametr = new DialogParameters();

			_dialogService.ShowModal("NewDevcieView", dialogParametr, x =>
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
					var newRomm = new RoomLineGraphInfo(new RoomData(), newDevice.IPAddress);
					RoomsCollection.Add(newRomm);
					var deviceClient = new DeviceClient(newDevice);
					_deviceClients.Add(deviceClient);
					Task.Run(async () => await SubscribeDeviceAsync(deviceClient));
					_xmlDeviceProvider.SaveDevices(RoomsCollection.Select(x => new Device { IPAddress = x.IPAddress }));

				}

			}
		}

	}
}
