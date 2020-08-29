using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;
using System;
using System.Windows;

namespace DHCPServer.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
	{
		public MainView()
		{
			InitializeComponent();

            // Create first source
            var source1 = new ObservableDataSource<Point>();
            // Set identity mapping of point in collection to point on plot
           // plotter.Viewport.Restrictions.Add(new CustomAxisRestriction(1));

             //line.DataSource = source1;
            
            // Add all three graphs. Colors are not specified and chosen random
         //  var graphT = plotterT.AddLineGraph(source1, Colors.Red, 2, "TEMP");
            //  var graphH = plotterH.AddLineGraph(source2, Colors.Green, 2, "HUM");
            // var graphD = plotterD.AddLineGraph(source3, Colors.Blue, 2, "DUST");
            //  var graphP = plotterP.AddLineGraph(source4, Colors.DarkSeaGreen, 2, "PRESS");
            // var graphACC = plotterACC.AddLineGraph(source5, Colors.CadetBlue, 2, "ACC"); 
            Point p1 = new Point(10, 20);
            Point p2 = new Point(11, 22);
            Point p3 = new Point(12, 25);
            Point p4 = new Point(13, 23);
            Point p5 = new Point(14, 22);
            source1.AppendAsync(Dispatcher, p1);
            source1.AppendAsync(Dispatcher, p2);
            source1.AppendAsync(Dispatcher, p3);
            source1.AppendAsync(Dispatcher, p4);
            source1.AppendAsync(Dispatcher, p5);
            ChartPlotter s = new ChartPlotter();
           // s.AddLineGraph()
        }
       
    }
    public class CustomAxisRestriction : ViewportRestrictionBase
    {
        private double xMin;
        public CustomAxisRestriction(double xMin)
        {
            this.xMin = xMin;
        }
        public override Rect Apply(Rect oldDataRect, Rect newDataRect, Viewport2D viewport)
        {
            newDataRect.X = Math.Max(newDataRect.X, xMin);
            return newDataRect;
        }
    }
}
