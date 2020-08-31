using DHCPServer.Core.Extensions;
using DHCPServer.Dialogs.Extenstions;
using DHCPServer.Models;
using DHCPServer.Models.Enums;
using DHCPServer.Models.Infrastructure;
using DHCPServer.Models.Repositories;
using DHCPServer.Services;
using DHCPServer.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
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
	public class GraphViewModel : BindableBase
	{
		#region Fields
		private readonly IDialogService _dialogService;
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

		#endregion


		public GraphViewModel(IDialogService dialogService, IRoomRepository roomRepository, XmlDeviceProvider xmlDeviceProvider)
		{
			OpenNewDevcieViewCommand = new DelegateCommand(OpenNewDevcieView);
			DeleteRoomCommand = new DelegateCommand<RoomLineGraphInfo>(DeleteRoom);
			_dialogService = dialogService;
			_roomRepository = roomRepository;
			_xmlDeviceProvider = xmlDeviceProvider;
			var devices = _xmlDeviceProvider.GetDevices();
			tokenSource = new CancellationTokenSource();
			RoomsCollection = new ObservableCollection<RoomLineGraphInfo>(_xmlDeviceProvider.CastDevices(devices).ToRoomLineGraphInfo());
			_timer = new DispatcherTimer();
			_timer.Interval = new TimeSpan(1, 0, 0);
			_timer.Tick += _timer_Tick;
			_timer.Start();
			_deviceClients = new List<DeviceClient>(RoomsCollection
				.Select(x => new Device { IPAddress = x.IPAddress })
				.ToDeviceClient());
			Task.Run(async () => await StartListenAsync()).Wait();
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
			//foreach (var room in RoomsCollection)
			//{
			//	room.AddToCollections();
			//}

			Task.Run(async () =>
			{
				await _roomRepository.SaveAsync(RoomsCollection);
			});

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
