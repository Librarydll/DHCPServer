using DHCPServer.Core.Events;
using DHCPServer.Core.Events.Model;
using DHCPServer.Core.Extensions;
using DHCPServer.Dialogs.Extenstions;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace DHCPServer.ViewModels
{
	public class DeviceInformationViewModel : DeviceViewModelBase<RoomLineGraphInfo,RoomInfo> 
	{
		#region Fields
		private readonly IRoomRepository _roomRepository;
		private readonly IEventAggregator _eventAggregator;
		#endregion

		#region Commands
		public DelegateCommand<RoomLineGraphInfo> DeleteRoomCommand { get; set; }
		public DelegateCommand<RoomLineGraphInfo> OpenGraphCommand { get; set; }
		public DelegateCommand<RoomLineGraphInfo> OpenCalibrationCommand { get; set; }
		#endregion

		public DeviceInformationViewModel(IDialogService dialogService,
			IRoomRepository roomRepository,
			IActiveDeviceRepository activeDeviceRepository,
			IReportRepository reportRepository,
			ILogger logger,
			IEventAggregator eventAggregator):base(dialogService,activeDeviceRepository,logger)
		{
			OpenNewDevcieViewCommand = new DelegateCommand(OpenNewDevcieView);
			DeleteRoomCommand = new DelegateCommand<RoomLineGraphInfo>(DeleteRoom);
			OpenGraphCommand = new DelegateCommand<RoomLineGraphInfo>(OpenGraph);
			OpenCalibrationCommand = new DelegateCommand<RoomLineGraphInfo>(OpenCalibration);

			_roomRepository = roomRepository;
			_eventAggregator = eventAggregator;

		
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
				var tasks = new Task[RoomsCollection.Count];
				for (int i = 0; i < RoomsCollection.Count; i++)
				{
					RoomsCollection[i].AddToCollectionEvent += DeviceInformationViewModel_AddToCollectionEvent;
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

		}
		public override void DeviceInformationViewModel_AddToCollectionEvent(ActiveDevice active, RoomInfo room)
		{
			Task.Run(async() =>
			{
				await _roomRepository.SaveAsync(room);

			}).ContinueWith(t =>
			{

				_logger.Error("Не удлаось добавить данные в {0}",active?.IPAddress);
				_logger.Error("Ошибка {0}", t.Exception?.Message);
				_logger.Error("Ошибка {0}", t.Exception?.InnerException);

			}, TaskContinuationOptions.OnlyOnFaulted);
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