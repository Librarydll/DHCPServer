﻿using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.ViewModels
{
	public class DeviceArchiveViewModel : BindableBase
	{
		private readonly IActiveDeviceRepository _activeDeviceRepository;
		private readonly IDialogService _dialogService;
		private IEnumerable<ActiveDevice> _devices = null;
		#region Properties
		private DateTimeSpanFilter _dateTimeSpan = new DateTimeSpanFilter();

		public DateTimeSpanFilter DateTimeSpan
		{
			get { return _dateTimeSpan; }
			set { SetProperty(ref _dateTimeSpan, value); }
		}

		private ObservableCollection<ActiveDevice> _devicesForViewCollection;
		public ObservableCollection<ActiveDevice> DevicesForViewCollection
		{
			get { return _devicesForViewCollection; }
			set { SetProperty(ref _devicesForViewCollection, value); }
		}

		private ObservableCollection<ActiveDevice> _devicesCollection;
		public ObservableCollection<ActiveDevice> DevicesCollection
		{
			get { return _devicesCollection; }
			set { SetProperty(ref _devicesCollection, value); }
		}
		private ActiveDevice _selectedDevice;
		public ActiveDevice SelectedDevice
		{
			get { return _selectedDevice; }
			set { SetProperty(ref _selectedDevice, value); SelectedDeviceChangeEvent(value); }
		}


		#endregion

		#region Command
		public DelegateCommand FilterCommand { get; set; }
		public DelegateCommand<ActiveDevice> OpenGraphCommand { get; set; }

		#endregion
		public DeviceArchiveViewModel(IActiveDeviceRepository activeDeviceRepository, IDialogService dialogService)
		{
			FilterCommand = new DelegateCommand(async () => await Filter());
			DevicesForViewCollection = new ObservableCollection<ActiveDevice>();
			DevicesCollection = new ObservableCollection<ActiveDevice>();
			_activeDeviceRepository = activeDeviceRepository;
			_dialogService = dialogService;
			OpenGraphCommand = new DelegateCommand<ActiveDevice>(OpenGraph);

		}

		private async Task Filter()
		{
			DevicesForViewCollection.Clear();
			DevicesCollection.Clear();
			IEnumerable<ActiveDevice> groupedDevices = null;
			if (DateTimeSpan.IsDateValidate())
			{
				_devices = await _activeDeviceRepository.GetActiveDevicesByDate(DateTimeSpan.FromDate, DateTimeSpan.ToDate);
				groupedDevices = _devices.GroupBy(x => x.IPAddress)
					.Select(x => x.First());
			}
			if (groupedDevices != null)
			{
				DevicesCollection = new ObservableCollection<ActiveDevice>(groupedDevices);
			}

		}

		private void OpenGraph(ActiveDevice activeDevice)
		{
			var room = new RoomLineGraphInfo(activeDevice);
			var dialogParametr = new DialogParameters
			{
				{ "model", room },
				{"date",DateTimeSpan }
			};

			_dialogService.Show("GraphView", dialogParametr, x =>
			{
			});
		}

		private void SelectedDeviceChangeEvent(ActiveDevice value)
		{
			if (value != null)
			{
				value.IsAdded = !value.IsAdded;
				var d = DevicesForViewCollection.FirstOrDefault(x => x.IPAddress == value.IPAddress);
				if (d != null && !value.IsAdded)
					DevicesForViewCollection.Remove(d);
				else if (value.IsAdded)
				{
					DevicesForViewCollection.Add(value);
				}
			}

		}
	}
}