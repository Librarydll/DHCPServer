using DHCPServer.Core;
using DHCPServer.Core.Extensions;
using DHCPServer.Dialogs.Extenstions;
using DHCPServer.Models;
using DHCPServer.Models.Context;
using DHCPServer.Models.Enums;
using DHCPServer.Models.Infrastructure;
using DHCPServer.Models.Repositories;
using DHCPServer.Services;
using DHCPServer.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DHCPServer.ViewModels
{
	public class MainViewModel : BindableBase
	{
		#region Fields
		private readonly IRegionManager _regionManager;
		private readonly  GraphDataView _graphDataView = new GraphDataView();
		private readonly  DeviceInformationView _deviceInformationView = new DeviceInformationView();
		private readonly ListDevicesView _listDevicesView = new ListDevicesView();
		#endregion

		#region Commands
		public DelegateCommand OpenGraphDataViewCommand { get; set; }
		public DelegateCommand OpenDeviceInformationViewCommand { get; set; }
		public DelegateCommand OpenDeviceViewCommand { get; set; }
		#endregion
		#region BindingProperties

		#endregion
		public MainViewModel(IRegionManager regionManager)
		{
			OpenGraphDataViewCommand = new DelegateCommand(OpenGraphDataView);
			OpenDeviceInformationViewCommand = new DelegateCommand(OpenDeviceInformationView);
			OpenDeviceViewCommand = new DelegateCommand(OpenDeviceView);
			_regionManager = regionManager;

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
