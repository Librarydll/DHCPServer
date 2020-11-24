using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DHCPServer.Dialogs
{
	public class FilterViewModelDialog : DialogViewModelBase
	{
		private readonly IDeviceRepository _deviceRepository;

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
		public FilterViewModelDialog(IDeviceRepository deviceRepository)
		{
			_deviceRepository = deviceRepository;
			Title = "Фильтр";
		}
		public override void OnDialogOpened(IDialogParameters parameters)
		{
			Task.Run(async () =>
			{
				var _devices = await _deviceRepository.GetAllAsync();
				DevicesColleciton = new ObservableCollection<ActiveDevice>(_devices.Select(x=>new ActiveDevice(x)));
			});

		}

		protected override void CloseDialogOnOk(IDialogParameters parameters)
		{
			if (!DateTimeSpan.IsDateValidate())
			{
				MessageBox.Show("Выбранное время неправильное");
				return;
			}

			Result = ButtonResult.OK;
			parameters = new DialogParameters
			{
				{ "model", DevicesColleciton },
				{ "date", DateTimeSpan }
			};


			CloseDialog(parameters);
		}

	}
}
