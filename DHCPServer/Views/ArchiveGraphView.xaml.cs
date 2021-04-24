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
    /// Interaction logic for ArchiveGraphView.xaml
    /// </summary>
    public partial class ArchiveGraphView : UserControl
    {
        public ArchiveGraphView()
        {
            InitializeComponent();
        }
        private void Plot1_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var vm = this.DataContext as ArchiveGraphViewModel;
            vm.TemperatureLineMouseWheelEventHandler(e);
        }



        private void Plot2_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var vm = this.DataContext as ArchiveGraphViewModel;
            vm.HumidityLineMouseWheelEventHandler(e);
        }
    }
}
