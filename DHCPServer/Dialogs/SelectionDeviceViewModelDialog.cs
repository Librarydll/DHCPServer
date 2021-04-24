using DHCPServer.Core.Extensions;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DHCPServer.Dialogs
{
	public class SelectionDeviceViewModelDialog : DialogViewModelBase
	{
		private IEnumerable<Device> _devices;
		private readonly IDeviceRepository _deviceRepository;
		private readonly IReportRepository _reportRepository;
		private readonly IActiveDeviceRepository _activeDeviceRepository;
		private Report _unChangedReport = null;

		private ObservableCollection<ActiveDevice> _devicesColleciton;

		public ObservableCollection<ActiveDevice> DevicesColleciton
		{
			get { return _devicesColleciton; }
			set { SetProperty(ref _devicesColleciton, value); }
		}

		private DateTimeSpanFilter _dateTimeSpan = new DateTimeSpanFilter();

		public DateTimeSpanFilter DateTimeSpan
		{
			get { return _dateTimeSpan; }
			set { SetProperty(ref _dateTimeSpan, value); }
		}

		private ObservableCollection<Report> _reportsCollection;
		public ObservableCollection<Report> ReportsCollection
		{
			get { return _reportsCollection; }
			set { SetProperty(ref _reportsCollection, value); }
		}

		private Report _selectedReport;
		public Report SelectedReport
		{
			get { return _selectedReport; }
			set 
			{
				if(SetProperty(ref _selectedReport, value))
				{
					SelectedReportChanged(value);
				}
			}
		}

		private int _selectedReportIndex=0;
		public int SelectedReportIndex
		{
			get { return _selectedReportIndex; }
			set { SetProperty(ref _selectedReportIndex, value); }
		}

		



		private Report _report;
		public Report Report
		{
			get { return _report; }
			set { SetProperty(ref _report, value); }
		}

		public DelegateCommand CreateNewReportCommand { get; set; }

		public SelectionDeviceViewModelDialog(
			IDeviceRepository deviceRepository,
			IReportRepository reportRepository,
			IActiveDeviceRepository activeDeviceRepository)
		{
			_deviceRepository = deviceRepository;
			_reportRepository = reportRepository;
			_activeDeviceRepository = activeDeviceRepository;
			Title = "Создание архива";
			CreateNewReportCommand = new DelegateCommand(CreateNewReportHandler);
		}

		private void CreateNewReportHandler()
		{
			Report = new Report
			{
				FromTime = DateTime.Now,
				Days = 1
			};
			FillCollection();
		}

		public override void OnDialogOpened(IDialogParameters parameters)
		{
			Task.Run(async () =>
			{
				var reports = await _reportRepository.GetActiveReportsWithDevices();
				ReportsCollection = new ObservableCollection<Report>(reports);
				_devices = await _deviceRepository.GetAllAsync(); 
				Report = ReportsCollection.FirstOrDefault();


				if (Report == null) CreateNewReportHandler();

				_unChangedReport = new Report(Report);
			});

		}

		protected override void CloseDialogOnOk(IDialogParameters parameters)
		{

			if (string.IsNullOrWhiteSpace(Report.Title)) return;

			Result = ButtonResult.OK;
            parameters = new DialogParameters
            {
                { "model", DevicesColleciton },
                { "report", Report }
            };

            HandleReport();

			CloseDialog(parameters);
		}

		private void SelectedReportChanged(Report report)
		{
			Report = report;
			FillCollection();
		//	Task.Run( async()=> await FillCollection(report.Id));

		}

		private void HandleReport()
		{
			Report.LastUpdated = DateTime.Now;
			if (Report.Id == 0)
			{
				Report.ActiveDevices = DevicesColleciton.Where(x => x.IsAdded).ToList();
				_reportRepository.CreateReport(Report).Wait();
			}
			else if(_unChangedReport.IsEdited(Report))
			{
				_reportRepository.UpdateAsync(Report).Wait();
			}
			foreach (var device in DevicesColleciton)
			{
				device.ReportId = Report.Id;
				device.Report = Report;
			}
		}

		

		private void FillCollection()
        {
			var activeDevices = Report.DistinctActiveDevice(ReportsCollection,_devices).ActiveDevices;
			DevicesColleciton = new ObservableCollection<ActiveDevice>(activeDevices);

		}

	}
}
