using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DHCPServer.Core.Graph
{
	public class LineGraphProvider:INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private ObservableDataSource<Point> _temperaturePointDataSource;
		public ObservableDataSource<Point> TemperaturePointDataSource
		{
			get { return _temperaturePointDataSource; }
			set { _temperaturePointDataSource = value; OnPropertyChanged(); }
		}
		private ObservableDataSource<Point> _humidityPointDataSource;
		public ObservableDataSource<Point> HumidityPointDataSource
		{
			get { return _humidityPointDataSource; }
			set { _humidityPointDataSource = value; OnPropertyChanged(); }
		}


		public LineGraphProvider()
		{
			TemperaturePointDataSource = new ObservableDataSource<Point>();
			HumidityPointDataSource = new ObservableDataSource<Point>();
		}


		public void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}
	}
}
