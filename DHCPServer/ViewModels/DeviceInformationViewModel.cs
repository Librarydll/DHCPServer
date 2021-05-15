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
    public class DeviceInformationViewModel : DeviceViewModelBase<RoomLineGraphInfo, RoomInfo>
    {
        #region Fields
        private readonly IRoomRepository _roomRepository;
        private readonly IEventAggregator _eventAggregator;
        #endregion

        #region Commands
        public DelegateCommand<RoomLineGraphInfo> DeleteRoomCommand { get; set; }
        public DelegateCommand<RoomLineGraphInfo> OpenGraphCommand { get; set; }
        public DelegateCommand<RoomLineGraphInfo> OpenCalibrationCommand { get; set; }
        public DelegateCommand OpenReportViewCommand { get; set; }
        public DelegateCommand CloseReportViewCommand { get; set; }
        public DelegateCommand<RoomLineGraphInfo> DeviceSwapCommand { get; set; }
        #endregion

        public DeviceInformationViewModel(IDialogService dialogService,
            IRoomRepository roomRepository,
            IActiveDeviceRepository activeDeviceRepository,
            IReportRepository reportRepository,
            ILogger logger,
            IEventAggregator eventAggregator) : base(dialogService, activeDeviceRepository, logger, Domain.Enumerations.DeviceType.Default)
        {
            OpenNewDevcieViewCommand = new DelegateCommand(OpenNewDevcieView);
            DeleteRoomCommand = new DelegateCommand<RoomLineGraphInfo>(DeleteRoom);
            OpenGraphCommand = new DelegateCommand<RoomLineGraphInfo>(OpenGraph);
            OpenCalibrationCommand = new DelegateCommand<RoomLineGraphInfo>(OpenCalibration);
            OpenReportViewCommand = new DelegateCommand(OpenReportView);
            DeviceSwapCommand = new DelegateCommand<RoomLineGraphInfo>(OpenSwapDeviceView);
            CloseReportViewCommand = new DelegateCommand(OpenCloseReportView);
            _roomRepository = roomRepository;
            _eventAggregator = eventAggregator;


            _eventAggregator.GetEvent<DeviceUpdateEvent>().Subscribe(DeviceUpdateEventHandler);
        }

        private void OpenCloseReportView()
        {
            ICollection<int> deletedReports = new List<int>();
            var args = new DialogParameters();
            args.Add("model", deletedReports);
            _dialogService.ShowModal("CloseReportView", args, callback =>
             {
                 if (deletedReports.Count > 0)
                 {
                     foreach (var room in RoomsCollection)
                     {

                         var id = deletedReports.FirstOrDefault(x => x == room.ActiveDevice.ReportId);

                         if (id > 0)
                         {
                             var device = new ActiveDevice
                             {
                                 DeviceType = Domain.Enumerations.DeviceType.Default,
                                 IPAddress = room.ActiveDevice.IPAddress,
                                 IsActive = true,
                                 IsAdded = true,
                                 Nick = room.ActiveDevice.Nick,
                             };
                             _activeDeviceRepository.CheckDevice(device).Wait();
                             room.ActiveDevice.Id = device.Id;
                             room.ActiveDevice.Report = null;
                             room.ActiveDevice.ReportId = 0;
                             room.ActiveDevice.IsSelected = false;
                            // deletedReports.Remove(id);
                         }
                     }
                   
                 }
             });

        }

        private void OpenSwapDeviceView(RoomLineGraphInfo roomLineGraphInfo)
        {
            var dialog = new DialogParameters();
            dialog.Add("model", roomLineGraphInfo);
            _dialogService.ShowModal("DeviceSwapView", dialog, callback =>
            {
                if (callback.Parameters.TryGetValue("model", out ActiveDevice activeDevice))
                {

                    roomLineGraphInfo.ActiveDevice.Report = null;
                    roomLineGraphInfo.ActiveDevice.ReportId = 0;
                    var room = RoomsCollection.FirstOrDefault(x => x.ActiveDevice.IPAddress == activeDevice.IPAddress);
                    room.ActiveDevice.Report = new Report(activeDevice.Report);
                    room.ActiveDevice.ReportId = activeDevice.Report.Id;
                }
            });
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
            room.DeviceId = active.Id;
            Task.Run(async () =>
            {
                await _roomRepository.SaveAsync(room);

            }).ContinueWith(t =>
            {

                _logger.Error("Не удлаось добавить данные в {0}", active?.IPAddress);
                _logger.Error("Ошибка {0}", t.Exception?.Message);
                _logger.Error("Ошибка {0}", t.Exception?.InnerException);

            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void OpenReportView()
        {
            var selectedDevices = RoomsCollection
                .Where(x => x.ActiveDevice.IsSelected)
                .Select(x => x.ActiveDevice);
            var args = new DialogParameters();
            args.Add("model", selectedDevices);

            _dialogService.ShowDialog("CreateReportView", args, x =>
            {
                if (x.Result == ButtonResult.OK)
                {
                    selectedDevices.Select(z => z.IsSelected = false).ToList();
                }
            });
        }


        private void OpenGraph(RoomLineGraphInfo roomLineGraphInfo)
        {
            var dialogParametr = new DialogParameters
            {
                { "model", roomLineGraphInfo }
            };

            _dialogService.Show("RealTimeGraphView", dialogParametr, x =>
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


        private void UpdateReportTimeInUI()
        {
            foreach (var room in RoomsCollection)
            {
                if (room.RoomInfo.ActiveDevice.Report != null)
                {
                    room.RoomInfo.ActiveDevice.Report.RaisePropertyChnagedDateTimePassed();
                }
            }
        }
    }

}