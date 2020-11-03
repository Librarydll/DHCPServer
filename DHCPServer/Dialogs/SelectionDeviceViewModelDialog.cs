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

		private ObservableCollection<Device> _devicesColleciton;

		public ObservableCollection<Device> DevicesColleciton
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


		public SelectionDeviceViewModelDialog(IDeviceRepository deviceRepository,IReportRepository reportRepository,IActiveDeviceRepository activeDeviceRepository)
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
				DevicesColleciton = new ObservableCollection<Device>(await _deviceRepository.GetAllAsync());
				var _addedDevices = await _activeDeviceRepository.GetAppropriateDevicesLists();

				//DevicesColleciton.CheckDevice(_addedDevices);
				var device = _addedDevices.FirstOrDefault();
				Report = await _reportRepository.GetLastReport();
				if (Report == null)
				{
					Report = new Report();
					Report.FromTime = DateTime.Now;
				}


				_unChangedReport = new Report(Report);
			});

		}

		protected override void CloseDialogOnOk(IDialogParameters parameters)
		{

			if (string.IsNullOrWhiteSpace(Report.Title)) return;

			Result = ButtonResult.OK;
			parameters = new DialogParameters();
			parameters.Add("model", DevicesColleciton);
			parameters.Add("report", Report);
			HandleReport();
			foreach (var device in DevicesColleciton.Where(x=>x.ActiveDevice.IsAdded))
			{
				device.ActiveDevice.ReportId = Report.Id;
			}
			CloseDialog(parameters);
		}

		private void HandleReport()
		{
			if (Report.Id == 0)
			{
				Report.LastUpdated = DateTime.Now;
				_reportRepository.CreateAsync(Report).Wait();
			}
			else if(_unChangedReport.IsEdited(Report))
			{
				Report.LastUpdated = DateTime.Now;
				_reportRepository.UpdateAsync(Report).Wait();
			}

		}
	}
}
