using DHCPServer.Dialogs.Extenstions;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.ViewModels
{
    public class DeviceArchiveViewModel : BindableBase
    {
        private readonly IActiveDeviceRepository _activeDeviceRepository;
        private readonly IDialogService _dialogService;
        private IEnumerable<ActiveDevice> _devices = null;
        #region Properties
        private DateTimeSpanFilter _dateTimeSpan = new DateTimeSpanFilter();

        public DateTimeSpanFilter DateTimeSpan
        {
            get { return _dateTimeSpan; }
            set { SetProperty(ref _dateTimeSpan, value); }
        }

        private ObservableCollection<ActiveDevice> _devicesForViewCollection;
        public ObservableCollection<ActiveDevice> DevicesForViewCollection
        {
            get { return _devicesForViewCollection; }
            set { SetProperty(ref _devicesForViewCollection, value); }
        }

        private ObservableCollection<ActiveDevice> _devicesCollection;
        public ObservableCollection<ActiveDevice> DevicesCollection
        {
            get { return _devicesCollection; }
            set { SetProperty(ref _devicesCollection, value); }
        }
        private ActiveDevice _selectedDevice;
        public ActiveDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set { SetProperty(ref _selectedDevice, value); SelectedDeviceChangeEvent(value); }
        }


        #endregion

        #region Command
        public DelegateCommand FilterCommand { get; set; }
        public DelegateCommand<ActiveDevice> OpenGraphCommand { get; set; }

        #endregion
        public DeviceArchiveViewModel(IActiveDeviceRepository activeDeviceRepository, IDialogService dialogService)
        {
            FilterCommand = new DelegateCommand(OpenFilterView);
            DevicesForViewCollection = new ObservableCollection<ActiveDevice>();
            DevicesCollection = new ObservableCollection<ActiveDevice>();
            _activeDeviceRepository = activeDeviceRepository;
            _dialogService = dialogService;
            OpenGraphCommand = new DelegateCommand<ActiveDevice>(OpenGraph);

        }

        private void OpenFilterView()
        {
            ObservableCollection<ActiveDevice> devices = null;
            _dialogService.ShowModal("FilterView", x =>
            {
                if (x.Result == ButtonResult.OK)
                {
                    devices = x.Parameters.GetValue<ObservableCollection<ActiveDevice>>("model");
                    DateTimeSpan = x.Parameters.GetValue<DateTimeSpanFilter>("date");
                }
            });


            if (devices != null)
            {
                Task.Run(async () =>
                {
                    _devices = await _activeDeviceRepository.GetActiveDevicesByDate(DateTimeSpan.FromDate, DateTimeSpan.ToDate);
                    var allDevices = _devices.GroupBy(x => x.IPAddress)
                        .Select(x => x.First());
                    DevicesForViewCollection = new ObservableCollection<ActiveDevice>();
                    foreach (var d in devices.Where(x => x.IsAdded))
                    {
                        var active = allDevices.FirstOrDefault(x => x.IPAddress == d.IPAddress);
                        if (active != null)
                            DevicesForViewCollection.Add(active);
                    }
                    // var d = devices.Where(x => allDevices.Any(z => z.IPAddress == x.IPAddress) && x.IsAdded);
                    //   DevicesForViewCollection = new ObservableCollection<ActiveDevice>(d);

                });

            }
        }

        private async Task Filter()
        {
            DevicesForViewCollection.Clear();
            DevicesCollection.Clear();
            IEnumerable<ActiveDevice> groupedDevices = null;
            if (DateTimeSpan.IsDateValidate())
            {
                _devices = await _activeDeviceRepository.GetActiveDevicesByDate(DateTimeSpan.FromDate, DateTimeSpan.ToDate);
                groupedDevices = _devices.GroupBy(x => x.IPAddress)
                    .Select(x => x.First());
            }
            if (groupedDevices != null)
            {
                DevicesCollection = new ObservableCollection<ActiveDevice>(groupedDevices);
            }

        }

        private void OpenGraph(ActiveDevice activeDevice)
        {
            var dialogParametr = new DialogParameters();
            dialogParametr.Add("date", DateTimeSpan);
            if (activeDevice.DeviceType == Domain.Enumerations.DeviceType.Default)
            {
                var room = new RoomLineGraphInfo(activeDevice);
                dialogParametr.Add("model", room);

                _dialogService.Show("GraphView", dialogParametr, x =>
                {
                });
            }
            if (activeDevice.DeviceType == Domain.Enumerations.DeviceType.Multi)
            {
                var room = new MultiRoomLineGraphInfo(activeDevice);
                dialogParametr.Add("model", room);
                _dialogService.Show("MultiGraphView", dialogParametr, x =>
                {
                });
            }

        }

        private void SelectedDeviceChangeEvent(ActiveDevice value)
        {
            if (value != null)
            {
                value.IsAdded = !value.IsAdded;
                var d = DevicesForViewCollection.FirstOrDefault(x => x.IPAddress == value.IPAddress);
                if (d != null && !value.IsAdded)
                    DevicesForViewCollection.Remove(d);
                else if (value.IsAdded)
                {
                    DevicesForViewCollection.Add(value);
                }
            }

        }
    }
}
