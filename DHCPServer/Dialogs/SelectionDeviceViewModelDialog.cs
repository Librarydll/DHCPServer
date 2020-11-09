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
			Task.Run(async () => await FillCollection(0));
		}

		public override void OnDialogOpened(IDialogParameters parameters)
		{
			Task.Run(async () =>
			{
				var rs = await _reportRepository.GetActiveReports();
				ReportsCollection = new ObservableCollection<Report>(rs);

				Report = ReportsCollection.FirstOrDefault();
				if (Report != null)
				{
					await FillCollection(Report.Id);
				}
				else
				{
					Report = new Report
					{
						FromTime = DateTime.Now,
						Days = 1
					};
				}


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
			Task.Run( async()=> await FillCollection(report.Id));
		}

		private void HandleReport()
		{
			if (Report.Id == 0)
			{
				Report.LastUpdated = DateTime.Now;
				_reportRepository.CreateReport(Report, DevicesColleciton.Where(x=>x.IsAdded)).Wait();
			}
			else if(_unChangedReport.IsEdited(Report))
			{
				Report.LastUpdated = DateTime.Now;
				_reportRepository.UpdateAsync(Report).Wait();
			}
			foreach (var device in DevicesColleciton)
			{
				device.ReportId = Report.Id;
				device.Report = Report;
			}
		}

		private async Task FillCollection(int id)
		{
			var devices = await _deviceRepository.GetAllAsync();
			var activeDevices = await _activeDeviceRepository.GetActiveDevicesByReportId(id);
			DevicesColleciton = new ObservableCollection<ActiveDevice>(devices.CreateActiveDevices(activeDevices));
		}
	}
}
