using DHCPServer.Core.Events;
using DHCPServer.Core.Events.Model;
using DHCPServer.Dialogs.Extenstions;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models;
using DHCPServer.Models.Repositories;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DHCPServer.ViewModels
{
	public class ListDevicesViewModel : BindableBase
	{
		private readonly IDialogService _dialogService;
		private readonly IDeviceRepository _deviceRepository;
		private readonly ILogger _logger;
		private readonly IEventAggregator _eventAggregator;

		public DelegateCommand CreateNewDeviceCommand { get; set; }
		public DelegateCommand<Device> EditDeviceCommand { get; set; }
		public DelegateCommand<Device> DeleteDeviceCommand { get; set; }


		private ObservableCollection<Device> _devicesColleciton;
		public ObservableCollection<Device> DevicesColleciton
		{
			get { return _devicesColleciton; }
			set { SetProperty(ref _devicesColleciton, value); }
		}

		public ListDevicesViewModel(IDialogService dialogService,IDeviceRepository deviceRepository,ILogger logger,IEventAggregator eventAggregator)
		{
			CreateNewDeviceCommand = new DelegateCommand(CreateNewDevice);
			EditDeviceCommand = new DelegateCommand<Device>(EditDevice);
			DeleteDeviceCommand = new DelegateCommand<Device>(DeleteDevice);
			_dialogService = dialogService;
			_deviceRepository = deviceRepository;
			_logger = logger;
			_eventAggregator = eventAggregator;
			Task.Run(async () =>
			{
				DevicesColleciton = new ObservableCollection<Device>(await deviceRepository.GetAllAsync());
			});
			
		}

		private void CreateNewDevice()
		{
			Device newDevice = null;

			_dialogService.ShowModal("NewDeviceView", x =>
			{
				if (x.Result == ButtonResult.OK)
				{
					newDevice = x.Parameters.GetValue<Device>("model");
				}
			});


			
			if (newDevice != null)
			{
				var d = DevicesColleciton.FirstOrDefault(x => x.IPAddress == newDevice.IPAddress);
				if (d != null)
				{
					MessageBox.Show("Такой девайс уже существует");
					return;
				}
				Task.Run(async () =>
				{
					newDevice = await _deviceRepository.CreateAsync(newDevice);
					if (newDevice != null)
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							DevicesColleciton.Add(newDevice);
						});
					}

				}).ContinueWith(t => {

					_logger.Error("Не удлаось создать новый девайс ip {0}", newDevice?.IPAddress);
					_logger.Error("Ошибка {0}", t.Exception?.Message);
					_logger.Error("Ошибка {0}", t.Exception?.InnerException);
				},TaskContinuationOptions.OnlyOnFaulted);
			}
		}
		private void EditDevice(Device device)
		{
			if (device == null)
			{
				MessageBox.Show("Выберите сначала устройство");
				return;
			}

			Device updatedDevice = null;
			var dialogParametr = new DialogParameters();
			dialogParametr.Add("model", device);
			_dialogService.ShowModal("NewDeviceView", dialogParametr, x =>
			{
				if (x.Result == ButtonResult.OK)
				{
					updatedDevice = x.Parameters.GetValue<Device>("model");
				}
			});

			if (updatedDevice != null)
			{
				Task.Run(async () =>
				{
					var b = await _deviceRepository.UpdateAsync(updatedDevice);

					if (b)
					{

						_eventAggregator.GetEvent<DeviceUpdateEvent>().Publish(new DeviceEventModel
						{
							NewValue = updatedDevice,
							OldValue = device
						});
						device = updatedDevice;
					}
				}).ContinueWith(t => {

					_logger.Error("Не удлаось изменить девайс ip {0}", device?.IPAddress);
					_logger.Error("Ошибка {0}", t.Exception?.Message);
				}, TaskContinuationOptions.OnlyOnFaulted);
			}

		}
		private void DeleteDevice(Device device)
		{
			if (device == null)
			{
				MessageBox.Show("Выберите сначала устройство");
				return;
			}

			var msgResult = MessageBox.Show("Вы действительно хотите удалить этот девайс?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (msgResult ==MessageBoxResult.Yes)
			{
				Task.Run(async () =>
				{
					var b = await _deviceRepository.DeleteAsync(device);

					if (b)
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							DevicesColleciton.Remove(device);
						});
					}
				}).ContinueWith(t => {

					_logger.Error("Не удлаось удалить девайс ip {0}", device?.IPAddress);
					_logger.Error("Ошибка {0}", t.Exception?.Message);
				}, TaskContinuationOptions.OnlyOnFaulted);
			}

		}
	}
}
