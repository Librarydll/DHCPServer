using DHCPServer.Models.Infrastructure;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DHCPServer.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private bool _manuallyClosed = false;
        public MainView()
        {   
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            _manuallyClosed = true;

            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_manuallyClosed)
            {
                e.Cancel = true;
                wind.ShowInTaskbar = false;
                wind.WindowState = WindowState.Minimized;
            }
        }
    }

  
}
