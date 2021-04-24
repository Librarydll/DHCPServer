using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DHCPServer.Dialogs
{
    public class DeviceSwapViewModel : DialogViewModelBase
    {
        public bool HasNext => CurrentIndex < _activeDeivces.Count - 1;


        public bool HasPrevious => CurrentIndex > 0;

        private int _currentIndex = 0;
        public int CurrentIndex
        {
            get { return _currentIndex; }
            set
            {
                SetProperty(ref _currentIndex, value);
                RaisePropertyChanged("HasPrevious");
                RaisePropertyChanged("HasNext");
            }
        }
        private List<ActiveDevice> _activeDeivces = new List<ActiveDevice>();
        private readonly IActiveDeviceRepository _activeDeviceRepository;

        private ActiveDevice _currentDevice;
        public ActiveDevice CurrentDevice
        {
            get { return _currentDevice; }
            set { SetProperty(ref _currentDevice, value); }
        }

        private ActiveDevice _swappedDevice;
        public ActiveDevice SwappedDevice
        {
            get { return _swappedDevice; }
            set { 
                SetProperty(ref _swappedDevice, value);
                IsEnabled = SwappedDevice != null;
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); SwapDevice.RaiseCanExecuteChanged();  }
        }

        public DelegateCommand PreviousDevice => new DelegateCommand(ExecutePreviousDevice);


        public DelegateCommand NextDevice => new DelegateCommand(ExecuteNextDevice);


        public DelegateCommand SwapDevice => new DelegateCommand(async () => await ExecuteSwapDevice()).ObservesCanExecute(() => IsEnabled);

        public DeviceSwapViewModel(IActiveDeviceRepository activeDeviceRepository)
        {
            _activeDeviceRepository = activeDeviceRepository;
            Title = "Замена шкафа";

        }

        private void ExecutePreviousDevice()
        {
            CurrentIndex--;
            if (CurrentIndex < 0)
            {
                CurrentIndex = 0;
            }
            SwappedDevice = _activeDeivces[CurrentIndex];
        }
        private void ExecuteNextDevice()
        {
            CurrentIndex++;
            if (CurrentIndex >= _activeDeivces.Count)
            {
                CurrentIndex--;
            }
            SwappedDevice = _activeDeivces[CurrentIndex];
        }

        private async Task ExecuteSwapDevice()
        {
            try
            {
                await _activeDeviceRepository.SwapReportId(CurrentDevice, SwappedDevice);
                CloseDialogOnOk(null);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters != null)
            {
                CurrentDevice = parameters.GetValue<RoomLineGraphInfo>("model").ActiveDevice;

                Task.Run(async () =>
                {
                    var devices = await _activeDeviceRepository.GetActiveDeviceWithoutReports();

                    _activeDeivces = devices.ToList();
                    var existedDevice = _activeDeivces.FirstOrDefault(x => x.IPAddress == CurrentDevice.IPAddress);
                    if (existedDevice != null)
                    {
                        _activeDeivces.Remove(existedDevice);
                    }
                    RaisePropertyChanged("HasPrevious");
                    RaisePropertyChanged("HasNext");
                    SwappedDevice = _activeDeivces.FirstOrDefault();
                });
            }
        }

        protected override void CloseDialogOnOk(IDialogParameters parameters)
        {
            Result = ButtonResult.OK;
            parameters = new DialogParameters();
            SwappedDevice.Report = CurrentDevice.Report;
            SwappedDevice.ReportId = CurrentDevice.ReportId;
            parameters.Add("model", SwappedDevice);
            base.CloseDialogOnOk(parameters);
        }
    }
}
