using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.DTO;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Dialogs
{
    public class CreateReportViewModel : DialogViewModelBase
    {
        private readonly IReportRepository _reportRepository;

        private IEnumerable<ActiveDevice> _activeDevices;
        private Report _report;
        public Report Report
        {
            get { return _report; }
            set { SetProperty(ref _report, value); }
        }

        private ObservableCollection<ActiveDeviceIteration> _devicesCollection;

        public ObservableCollection<ActiveDeviceIteration> DevicesCollection
        {
            get { return _devicesCollection; }
            set { SetProperty(ref _devicesCollection, value); }
        }
        public DelegateCommand CreateReportCommand { get; set; }

        public CreateReportViewModel(IReportRepository reportRepository)
        {
            Report = new Report()
            {
                FromTime = DateTime.Now,
                Days = 16
            };
            Title = "Создание архива";
            CreateReportCommand = new DelegateCommand(async () => await ExecuteCreateReportCommand());
            _reportRepository = reportRepository;
        }

        private async Task ExecuteCreateReportCommand()
        {
            if (string.IsNullOrWhiteSpace(Report.Title)) return;
            if (DevicesCollection?.Count <= 0) return;
            Result = ButtonResult.OK;
            Report.ActiveDevices = _activeDevices.ToList();
            await _reportRepository.CreateReport(Report);
            CloseDialog(null);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            _activeDevices = parameters.GetValue<IEnumerable<ActiveDevice>>("model");
            DevicesCollection = new ObservableCollection<ActiveDeviceIteration>(_activeDevices.Map());
        }
    }
}
