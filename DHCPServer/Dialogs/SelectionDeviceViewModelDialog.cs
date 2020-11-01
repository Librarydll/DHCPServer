using DHCPServer.Core.Extensions;
using DHCPServer.Models;
using DHCPServer.Models.Infrastructure;
using DHCPServer.Models.Repositories;
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
	public class SelectionDeviceViewModelDialog : DialogViewModelBase
	{
		private readonly IDeviceRepository _deviceRepository;
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

		public SelectionDeviceViewModelDialog(IDeviceRepository deviceRepository)
		{
			_deviceRepository = deviceRepository;
		}

		public override void OnDialogOpened(IDialogParameters parameters)
		{
			Task.Run(async () =>
			{
				DevicesColleciton = new ObservableCollection<Device>(await _deviceRepository.GetAllAsync());
				_addedDevices = await _deviceRepository.GetDevicesLists();

				DevicesColleciton.CheckDevice(_addedDevices);

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
