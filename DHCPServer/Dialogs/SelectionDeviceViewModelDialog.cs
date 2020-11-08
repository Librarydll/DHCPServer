using DHCPServer.Core.Extensions;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure;
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

		private Report _report;
		public Report Report
		{
			get { return _report; }
			set { SetProperty(ref _report, value); }
		}


		public SelectionDeviceViewModelDialog(
			IDeviceRepository deviceRepository,
			IReportRepository reportRepository,
			IActiveDeviceRepository activeDeviceRepository)
		{
			_deviceRepository = deviceRepository;
			_reportRepository = reportRepository;
			_activeDeviceRepository = activeDeviceRepository;
			Title = "Добавление";
		}

		public override void OnDialogOpened(IDialogParameters parameters)
		{
			Task.Run(async () =>
			{
				Report = await _reportRepository.GetLastReport();
				if (Report == null)
				{
					Report = new Report();
					Report.FromTime = DateTime.Now;
					Report.Days = 1;
				}

				var devices = await _deviceRepository.GetAllAsync();
				var activeDevices = await _activeDeviceRepository.GetActiveDevicesByReportId(Report.Id);

				DevicesColleciton = new ObservableCollection<ActiveDevice>(devices.CreateActiveDevices(activeDevices));

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
		}
	}
}
