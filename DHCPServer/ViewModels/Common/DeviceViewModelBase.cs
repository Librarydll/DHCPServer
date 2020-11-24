using DHCPServer.Dialogs.Extenstions;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure.Common;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DHCPServer.ViewModels.Common
{
	public abstract class DeviceViewModelBase<TRoomLine,TRoom> : BindableBase
		where TRoomLine : RoomLineBase<ActiveDevice, TRoom>
		where TRoom : RoomInfo, new()
	{
		protected readonly IDialogService _dialogService;
		protected readonly IActiveDeviceRepository _activeDeviceRepository;
		protected readonly ILogger _logger;
		private ObservableCollection<TRoomLine> _roomsCollection;

		public ObservableCollection<TRoomLine> RoomsCollection
		{
			get { return _roomsCollection; }
			set { SetProperty(ref _roomsCollection, value); }
		}

		public DelegateCommand OpenNewDevcieViewCommand { get; set; }

		public DeviceViewModelBase(IDialogService dialogService,IActiveDeviceRepository activeDeviceRepository,ILogger logger)
		{
			OpenNewDevcieViewCommand = new DelegateCommand(OpenNewDevcieView);
			_dialogService = dialogService;
			_activeDeviceRepository = activeDeviceRepository;
			_logger = logger;
		}

		public virtual void OpenNewDevcieView()
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
					TRoomLine roomLine = Activator.CreateInstance(typeof(TRoomLine), newDevice) as TRoomLine;
					roomLine.AddToCollectionEvent += DeviceInformationViewModel_AddToCollectionEvent;
					RoomsCollection.Add(roomLine);

					Task.Run(async () =>
					{
						await _activeDeviceRepository.CheckDevice(newDevice);
						_logger.Information("Added new device to listen {0}", newDevice.IPAddress);
						
						await roomLine.InitializeDeviceAsync();

					}).ContinueWith(t =>
					{
						_logger.Error(t.Exception.Message);
						_logger.Error(t.Exception?.InnerException?.Message);
					}, TaskContinuationOptions.OnlyOnFaulted);

				}

			}


		}

		public virtual void DeviceInformationViewModel_AddToCollectionEvent(ActiveDevice active, TRoom room)
		{

		}

	}
}
