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
    /// Interaction logic for MultiGraphView.xaml
    /// </summary>
    public partial class MultiGraphView : UserControl
    {
        public MultiGraphView()
        {
            InitializeComponent();
        }

        private void Plot1_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var vm = this.DataContext as MultiGraphViewModelDialog;
            vm.LineMouseWheelEventHandler(e,1);
        }

        private void Plot2_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var vm = this.DataContext as MultiGraphViewModelDialog;
            vm.LineMouseWheelEventHandler(e, 2);
        }

        private void Plot3_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var vm = this.DataContext as MultiGraphViewModelDialog;
            vm.LineMouseWheelEventHandler(e, 3);
        }

        private void Plot4_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var vm = this.DataContext as MultiGraphViewModelDialog;
            vm.LineMouseWheelEventHandler(e, 4);
        }
    }
}
