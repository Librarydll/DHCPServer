using DHCPServer.Dialogs;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Services.Dialogs;
using Serilog;
using System.Windows;
using System.Windows.Controls;

namespace DHCPServer.Views
{
	/// <summary>
	/// Interaction logic for GraphView
	/// </summary>
	public partial class GraphView : UserControl
	{
		public GraphView()
		{
			InitializeComponent();
		}

		private void Plot1_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
		{
			var vm = this.DataContext as GraphViewModelDialog;
			vm.LineMouseWheelEventHandler(e);
		}
	}
}
