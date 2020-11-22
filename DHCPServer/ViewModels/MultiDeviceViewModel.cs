using DHCPServer.Dialogs.Extenstions;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure;
using DHCPServer.ViewModels.Common;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DHCPServer.ViewModels
{
	public class MultiDeviceViewModel : DeviceViewModelBase<MultiRoomLineGraphInfo,MultiRoomInfo>
	{

		public MultiDeviceViewModel(IDialogService dialogService,
			IActiveDeviceRepository activeDeviceRepository,
			ILogger logger,
			IEventAggregator eventAggregator) : base(dialogService, activeDeviceRepository, logger)
		{
			RoomsCollection = new ObservableCollection<MultiRoomLineGraphInfo>();
		}

		public async Task InitializeAsync()
		{
			RoomsCollection = new ObservableCollection<MultiRoomLineGraphInfo>();

			//RoomsCollection = new ObservableCollection<MultiRoomLineGraphInfo>();
			//var m = new MultiRoomLineGraphInfo(new ActiveDevice(new Device("192.168.1.50", "uuu")));
			//m.RoomInfo = new MultiRoomInfo
			//{
			//	Humidity = 1, Temperature = 2, HumidityMiddle = 3, TemperatureMiddle = 4, HumidityNord = 5, TemperatureNord = 6, HumidityProcess = 7, TemperatureProcess = 8
			//};
			//RoomsCollection.Add(m);
		}

		public override void OpenNewDevcieView()
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
					MultiRoomLineGraphInfo roomLine = new MultiRoomLineGraphInfo(newDevice);

					RoomsCollection.Add(roomLine);

					Task.Run(async () =>
					{
						await roomLine.InitializeDeviceAsync();

					}).ContinueWith(t =>
					{
						_logger.Error(t.Exception.Message);
						_logger.Error(t.Exception?.InnerException?.Message);
					}, TaskContinuationOptions.OnlyOnFaulted);

				}

			}
		}
	}
}
