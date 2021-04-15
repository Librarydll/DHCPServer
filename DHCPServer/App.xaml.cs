using DHCPServer.Dapper.Repositories;
using DHCPServer.Dialogs;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Models;
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
        public App()
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message+"\n"+e.Exception.InnerException?.Message);
            MessageBox.Show(e.Exception.StackTrace);
        }

        protected override Window CreateShell()
		{
			return Container.Resolve<MainView>();
		}
		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			 Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.WriteTo.File(@"logs\\log.log", rollingInterval: RollingInterval.Month)
			.CreateLogger();

			containerRegistry.RegisterInstance(Log.Logger);
			containerRegistry.RegisterSingleton<IRoomRepository, RoomRepository>();
			containerRegistry.RegisterSingleton<IDeviceRepository, DeviceRepository>();
			containerRegistry.RegisterSingleton<IReportRepository, ReportRepository>();
			containerRegistry.RegisterSingleton<IActiveDeviceRepository, ActiveDeviceRepository>();
			containerRegistry.RegisterSingleton<IMultiRoomRepository, MultiRoomRepository>();
			containerRegistry.RegisterSingleton<Transfer>();

			containerRegistry.RegisterDialog<NewDeviceView, NewDeviceViewModelDialog>();
			containerRegistry.RegisterDialog<GraphView, GraphViewModelDialog>();
			containerRegistry.RegisterDialog<SelectionDeviceView, SelectionDeviceViewModelDialog>();
			containerRegistry.RegisterDialog<CalibrationView, CalibrationViewModelDialog>();
			containerRegistry.RegisterDialog<SelectionDeviceViewOld, SelectionDeviceViewOldModelDialog>();
			containerRegistry.RegisterDialog<FilterView, FilterViewModelDialog>();
			containerRegistry.RegisterDialog<MultiGraphView, MultiGraphViewModelDialog>();
		}
	}
}
