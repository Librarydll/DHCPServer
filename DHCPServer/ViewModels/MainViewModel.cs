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
		private readonly GraphView _graphView = new GraphView();
		private readonly  GraphDataView _graphDataView = new GraphDataView();
		#endregion

		#region Commands
		public DelegateCommand OpenGraphViewCommand { get; set; }
		public DelegateCommand OpenGraphDataViewCommand { get; set; }
		#endregion
		#region BindingProperties

		#endregion
		public MainViewModel(IRegionManager regionManager)
		{
			OpenGraphViewCommand = new DelegateCommand(OpenGraphView);
			OpenGraphDataViewCommand = new DelegateCommand(OpenGraphDataView);
			_regionManager = regionManager;

		}

		private void OpenGraphDataView()
		{
			OpenViewBase(_graphDataView, nameof(GraphDataView));

		}

		private void OpenGraphView()
		{
			OpenViewBase(_graphView, nameof(GraphView));
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
