using DHCPServer.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DHCPServer.Views
{
    /// <summary>
    /// Interaction logic for RealTimeGraphView.xaml
    /// </summary>
    public partial class RealTimeGraphView : UserControl
    {
        public RealTimeGraphView()
        {
            InitializeComponent();
        }

        private void Plot1_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var vm = this.DataContext as RealTimeGraphViewModel;
            vm.TemperatureLineMouseWheelEventHandler(e);
        }

        private void Plot2_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var vm = this.DataContext as RealTimeGraphViewModel;
            vm.HumidityLineMouseWheelEventHandler(e);
        }
    }
}
