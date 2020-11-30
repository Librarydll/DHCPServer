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
		private readonly IMultiRoomRepository _multiRoomRepository;

		public DelegateCommand<MultiRoomLineGraphInfo> OpenFirstGraphCommand { get; set; }
		public DelegateCommand<MultiRoomLineGraphInfo> OpenSecondGraphCommand { get; set; }
		public DelegateCommand<MultiRoomLineGraphInfo> OpenThirdGraphCommand { get; set; }
		public DelegateCommand<MultiRoomLineGraphInfo> DeleteRoomCommand { get; set; }


		public MultiDeviceViewModel(IDialogService dialogService,
			IActiveDeviceRepository activeDeviceRepository,
			IMultiRoomRepository multiRoomRepository,
			ILogger logger,
			IEventAggregator eventAggregator) : base(dialogService, activeDeviceRepository, logger)
		{
			RoomsCollection = new ObservableCollection<MultiRoomLineGraphInfo>();
			OpenFirstGraphCommand = new DelegateCommand<MultiRoomLineGraphInfo>(OpenFirstGraph);
			OpenSecondGraphCommand = new DelegateCommand<MultiRoomLineGraphInfo>(OpenSecondGraph);
			OpenThirdGraphCommand = new DelegateCommand<MultiRoomLineGraphInfo>(OpenThirdGraph);
			DeleteRoomCommand = new DelegateCommand<MultiRoomLineGraphInfo>(DeleteRoom);

			_multiRoomRepository = multiRoomRepository;
		}

		private void OpenFirstGraph(MultiRoomLineGraphInfo roomInfo)
		{
			var r = new RoomLineGraphInfo(roomInfo.ActiveDevice, false)
			{
				GraphLineModel = roomInfo.GraphLineModelForDefault
			};
			OpenGraph(r);
			r?.Dispose();
		}
		private void OpenSecondGraph(MultiRoomLineGraphInfo roomInfo)
		{
			var r = new RoomLineGraphInfo(roomInfo.ActiveDevice, false)
			{
				GraphLineModel = roomInfo.GraphLineModelForMiddle
			}; 
			OpenGraph(r);
			r?.Dispose();
		}

		private void OpenThirdGraph(MultiRoomLineGraphInfo roomInfo)
		{
			var r = new RoomLineGraphInfo(roomInfo.ActiveDevice, false)
			{
				GraphLineModel = roomInfo.GraphLineModelForProcess
			}; 
			OpenGraph(r);
			r?.Dispose();
		}


		private void DeleteRoom(MultiRoomLineGraphInfo roomInfo)
		{
			RoomsCollection.Remove(roomInfo);
			roomInfo.ActiveDevice.IsAdded = false;
			_activeDeviceRepository.DeatachDevice(roomInfo.ActiveDevice).Wait();
			roomInfo?.Dispose();

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
					roomLine.AddToCollectionEvent += DeviceInformationViewModel_AddToCollectionEvent;
					Task.Run(async () =>
					{
						_logger.Information("multidevice added ip {0}", newDevice.IPAddress);
						await roomLine.InitializeDeviceAsync();

					}).ContinueWith(t =>
					{
						_logger.Error(t.Exception.Message);
						_logger.Error(t.Exception?.InnerException?.Message);
					}, TaskContinuationOptions.OnlyOnFaulted);

				}

			}
		}

		public override void DeviceInformationViewModel_AddToCollectionEvent(ActiveDevice active, MultiRoomInfo room)
		{
			room.DeviceId = active.Id;
			Task.Run(async () =>
			{
				await _multiRoomRepository.SaveAsync(room);

			}).ContinueWith(t =>
			{

				_logger.Error("Не удлаось добавить данные в {0}", active?.IPAddress);
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
	}
}
