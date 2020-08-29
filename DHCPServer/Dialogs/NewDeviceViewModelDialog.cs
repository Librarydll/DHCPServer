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
		public NewDeviceViewModelDialog()
		{
			Device = new Device() { IPAddress="192.168.1.1"};
		}
		protected override void CloseDialogOnOk(IDialogParameters parameters)
		{
			Result = ButtonResult.OK;
			parameters = new DialogParameters();
			parameters.Add("model", Device);
			CloseDialog(parameters);
		}
	}
}
