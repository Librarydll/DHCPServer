using DHCPServer.Core.Extensions;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure;
using Prism.Services.Dialogs;
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
				_addedDevices = await _deviceRepository.GetDevicesLists();

				DevicesColleciton.CheckDevice(_addedDevices);
				var device = _addedDevices.FirstOrDefault();
			//	if(device ==null)
				//Report = await _reportRepository.GetLastReport()
			});

		}

		protected override void CloseDialogOnOk(IDialogParameters parameters)
		{
			Result = ButtonResult.OK;
			parameters = new DialogParameters();
			parameters.Add("model", DevicesColleciton);
			CloseDialog(parameters);
		}


	}
}
