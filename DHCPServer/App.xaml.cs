using DHCPServer.Dialogs;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Models;
using DHCPServer.Models.Repositories;
using DHCPServer.Views;
using Prism.Ioc;
using Prism.Unity;
using Serilog;
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
			Log.Logger = new LoggerConfiguration()
				.WriteTo.File("logs\\log.log")
				.CreateLogger();
			containerRegistry.RegisterInstance(Log.Logger);
			containerRegistry.RegisterSingleton<IRoomRepository, RoomRepository>();
			containerRegistry.RegisterSingleton<IDeviceRepository, DeviceRepository>();
			containerRegistry.RegisterSingleton<IReportRepository, ReportRepository>();
			containerRegistry.RegisterSingleton<Transfer>();

			containerRegistry.RegisterDialog<NewDeviceView, NewDeviceViewModelDialog>();
			containerRegistry.RegisterDialog<GraphView, GraphViewModelDialog>();
			containerRegistry.RegisterDialog<SelectionDeviceView, SelectionDeviceViewModelDialog>();
			containerRegistry.RegisterDialog<CalibrationView, CalibrationViewModelDialog>();
		}
	}
}
