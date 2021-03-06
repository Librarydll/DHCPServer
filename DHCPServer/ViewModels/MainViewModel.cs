﻿using DHCPServer.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Threading.Tasks;

namespace DHCPServer.ViewModels
{
	public class MainViewModel : BindableBase
	{
		#region Fields
		private readonly IRegionManager _regionManager;
		private readonly  GraphDataView _graphDataView = new GraphDataView();
		private readonly  DeviceInformationView _deviceInformationView = new DeviceInformationView();
		private readonly ListDevicesView _listDevicesView = new ListDevicesView();
		//	private readonly ReportView _reportView = new ReportView();
		private readonly DeviceArchiveView _deviceArchiveView = new DeviceArchiveView();
		private readonly MultiDeviceView _multiDeviceView = new MultiDeviceView();
		#endregion

		#region Commands
		public DelegateCommand OpenGraphDataViewCommand { get; set; }
		public DelegateCommand OpenDeviceInformationViewCommand { get; set; }
		public DelegateCommand OpenDeviceViewCommand { get; set; }
		public DelegateCommand OpenReportViewCommand { get; set; }
		public DelegateCommand OpenMultiDeviceViewCommand { get; set; }
		#endregion
		#region BindingProperties

		#endregion
		public MainViewModel(IRegionManager regionManager)
		{
			OpenGraphDataViewCommand = new DelegateCommand(OpenGraphDataView);
			OpenDeviceInformationViewCommand = new DelegateCommand(OpenDeviceInformationView);
			OpenDeviceViewCommand = new DelegateCommand(OpenDeviceView);
			OpenReportViewCommand = new DelegateCommand(OpenReportView);
			OpenMultiDeviceViewCommand = new DelegateCommand(OpenMultiDeviceView);
			_regionManager = regionManager;
		}

		private void OpenMultiDeviceView()
		{
			OpenViewBase(_multiDeviceView, nameof(MultiDeviceView));
		}
		private void OpenReportView()
		{
			OpenViewBase(_deviceArchiveView, nameof(DeviceArchiveView));
		}

		private void OpenDeviceView()
		{
			OpenViewBase(_listDevicesView, nameof(ListDevicesView));
		}

		private void OpenDeviceInformationView()
		{
			OpenViewBase(_deviceInformationView, nameof(DeviceInformationView));

		}

		private void OpenGraphDataView()
		{
			OpenViewBase(_graphDataView, nameof(GraphDataView));
		}

		private void OpenViewBase(object view, string viewName, string regionName = "MainRegion")
		{
			if (view == null) return;

			if (!_regionManager.Regions[regionName].Views.Contains(view))
			{
				_regionManager.Regions[regionName].Add(view, viewName);
			}
			_regionManager.Regions[regionName].Activate(view);
		}

	}
}
