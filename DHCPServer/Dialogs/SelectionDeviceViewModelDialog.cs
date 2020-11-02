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
		private IEnumerable<Device> _addedDevices;


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

		private int _days;
		public int Days
		{
			get { return _days; }
			set { SetProperty(ref _days, value); }
		}

		public SelectionDeviceViewModelDialog(IDeviceRepository deviceRepository,IReportRepository reportRepository)
		{
			_deviceRepository = deviceRepository;
			_reportRepository = reportRepository;
		}

		public override void OnDialogOpened(IDialogParameters parameters)
		{
			Task.Run(async () =>
			{
				DevicesColleciton = new ObservableCollection<Device>(await _deviceRepository.GetAllAsync());
				var _addedDevices = await _deviceRepository.GetActiveDevicesLists();

				//DevicesColleciton.CheckDevice(_addedDevices);
				var device = _addedDevices.FirstOrDefault();
				Report = await _reportRepository.GetLastReport();

				if (Report != null)
				{
					Days = Report.Days;
				}
			});

		}

		protected override void CloseDialogOnOk(IDialogParameters parameters)
		{
			Result = ButtonResult.OK;
			parameters = new DialogParameters();
			parameters.Add("model", DevicesColleciton);
			CloseDialog(parameters);
		}

		private async Task ReportHandle()
		{
			Report.LastUpdated = DateTime.Now;
			//Report.ToTime = Report.FromTime.AddDays(Days);
		}


	}
}
