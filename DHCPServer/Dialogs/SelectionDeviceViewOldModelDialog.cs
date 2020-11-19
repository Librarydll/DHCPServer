using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DHCPServer.Dialogs
{
	public class SelectionDeviceViewOldModelDialog : DialogViewModelBase
	{
		private readonly IDeviceRepository _deviceRepository;

		private Device _selectedServer;
		public Device SelectedServer
		{
			get { return _selectedServer; }
			set { SetProperty(ref _selectedServer, value); }
		}


		private ObservableCollection<Device> _serverAddressCollection;

		public ObservableCollection<Device> ServerAddressCollection
		{
			get { return _serverAddressCollection; }
			set { SetProperty(ref _serverAddressCollection, value); }
		}
		public SelectionDeviceViewOldModelDialog(IDeviceRepository deviceRepository)
		{
			_deviceRepository = deviceRepository;

			Task.Run(async () =>
			{
				ServerAddressCollection = new ObservableCollection<Device>(await _deviceRepository.GetAllAsync());
			});
			Title = "Добавить";
		}

		protected override void CloseDialogOnOk(IDialogParameters parameters)
		{
			if (SelectedServer == null) return;
			Result = ButtonResult.OK;
			parameters = new DialogParameters();
			var activeDevice = new ActiveDevice(SelectedServer);
			activeDevice.IsAdded = true;
			activeDevice.IsActive = true;
			parameters.Add("model", activeDevice);
			CloseDialog(parameters);
		} 
	}
}
