﻿using DHCPServer.Dialogs.Extenstions;
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
			IEventAggregator eventAggregator) : base(dialogService, activeDeviceRepository, logger,Domain.Enumerations.DeviceType.Multi)
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
			OpenGraph(r,1);
			r?.Dispose();
		}
		private void OpenSecondGraph(MultiRoomLineGraphInfo roomInfo)
		{
			var r = new RoomLineGraphInfo(roomInfo.ActiveDevice, false)
			{
				GraphLineModel = roomInfo.GraphLineModelForMiddle
			}; 
			OpenGraph(r,2);
			r?.Dispose();
		}

		private void OpenThirdGraph(MultiRoomLineGraphInfo roomInfo)
		{
			var r = new RoomLineGraphInfo(roomInfo.ActiveDevice, false)
			{
				GraphLineModel = roomInfo.GraphLineModelForProcess
			}; 
			OpenGraph(r,4);
			r?.Dispose();
		}


		private void DeleteRoom(MultiRoomLineGraphInfo roomInfo)
		{
			RoomsCollection.Remove(roomInfo);
			roomInfo.ActiveDevice.IsAdded = false;
			_activeDeviceRepository.DeatachDevice(roomInfo.ActiveDevice).Wait();
			roomInfo?.Dispose();

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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="roomLineGraphInfo"></param>
		/// <param name="dataType">1-обычная,2-мид,3-норд,4-процесс</param>
		private void OpenGraph(RoomLineGraphInfo roomLineGraphInfo,int dataType)
		{
			var dialogParametr = new DialogParameters
			{
				{ "model", roomLineGraphInfo },
				{ "dataType" , dataType }				
			};


			_dialogService.Show("RealTimeGraphView", dialogParametr, x =>
			{
			});
		}
	}
}
