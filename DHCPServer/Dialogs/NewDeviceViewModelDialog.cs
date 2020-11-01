using DHCPServer.Domain.Models;
using DHCPServer.Models;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Dialogs
{
	public class NewDeviceViewModelDialog: DialogViewModelBase
	{
		private Device _device;
		public Device Device
		{
			get { return _device; }
			set { SetProperty(ref _device, value); }
		}

		private string _buttonContent;
		public string ButtonContent
		{
			get { return _buttonContent; }
			set { SetProperty(ref _buttonContent, value); }
		}
		public NewDeviceViewModelDialog()
		{
		}
		protected override void CloseDialogOnOk(IDialogParameters parameters)
		{
			Result = ButtonResult.OK;
			parameters = new DialogParameters();

			if (string.IsNullOrWhiteSpace(Device.IPAddress) || string.IsNullOrWhiteSpace(Device.Nick)) return;

			parameters.Add("model", Device);
			CloseDialog(parameters);
		}

		public override void OnDialogOpened(IDialogParameters parameters)
		{
			if (parameters != null)
			{
				Device = parameters.GetValue<Device>("model");
				ButtonContent = "Изменить";
			}
			else
			{
				Device = new Device() { IPAddress = "192.168.1.1" };
				ButtonContent = "Добавить";
			}
		}
	}
}
