using DHCPServer.Dialogs;
using DHCPServer.Models.Repositories;
using DHCPServer.Services;
using DHCPServer.Views;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DHCPServer
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : PrismApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainView>();
		}
		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			containerRegistry.RegisterSingleton<IRoomRepository, RoomRepository>();
			containerRegistry.RegisterSingleton<XmlDeviceProvider>();

			containerRegistry.RegisterDialog<NewDevcieView, NewDeviceViewModelDialog>();
		}
	}
}
