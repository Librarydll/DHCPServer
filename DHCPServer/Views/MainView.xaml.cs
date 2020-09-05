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
        private readonly List<OxyColor> colors = new List<OxyColor>
                                            {
                                                OxyColors.Red,
                                                OxyColors.Red,
                                                OxyColors.Red,
                                                OxyColors.Red,
                                                OxyColors.Red
                                            };
        private readonly List<MarkerType> markerTypes = new List<MarkerType>
                                                   {
                                                       MarkerType.None,
                                                       MarkerType.None,
                                                       MarkerType.None,
                                                       MarkerType.None,
                                                       MarkerType.None
                                                   };

        public PlotModel PlotModel { get; private set; } = new PlotModel();

        public MainView()
        {
            InitializeComponent();
            //SetUpModel();
            //LoadData();

            //Plot1.Model = PlotModel;
        }

        private void SetUpModel()
        {
            PlotModel.LegendTitle = "Legend";
            PlotModel.LegendOrientation = LegendOrientation.Horizontal;
            PlotModel.LegendPlacement = LegendPlacement.Outside;
            PlotModel.LegendPosition = LegendPosition.TopRight;
            PlotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            PlotModel.LegendBorder = OxyColors.Black;

            var dateAxis = new TimeSpanAxis() { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, IntervalLength = 50, Position = AxisPosition.Bottom, Title = "Date",StringFormat= "mm:ss" };
            PlotModel.Axes.Add(dateAxis);
            var valueAxis = new LinearAxis() { MajorGridlineStyle = LineStyle.Dot, MinorGridlineStyle = LineStyle.Dot, Title = "Value", Position = AxisPosition.Left, StartPosition = 0, MinimumMajorStep = 1, MajorStep = 5 ,MaximumRange=60,AbsoluteMaximum=70,AbsoluteMinimum=0};
            PlotModel.Axes.Add(valueAxis);
        }

        private void LoadData()
        {
            List<Measurement> measurements = Data.GetData();

            var dataPerDetector = measurements.GroupBy(m => m.DetectorId).ToList();

            foreach (var data in dataPerDetector)
            {
                var lineSerie = new LineSeries
                {
                    StrokeThickness = 2,
                    MarkerSize = 3,
                    MarkerStroke = colors[data.Key],
                    MarkerType = markerTypes[data.Key],
                    CanTrackerInterpolatePoints = false,
                    Title = string.Format("Detector {0}", data.Key),
                };

                data.ToList().ForEach(d => lineSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(d.DateTime), d.Value)));
                PlotModel.Series.Add(lineSerie);
            }
        }
    }

    public class Data
    {
        public static List<Measurement> GetData()
        {
            var measurements = new List<Measurement>();

            var startDate = DateTime.Now.AddMinutes(-10);
            var r = new Random();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    measurements.Add(new Measurement() { DetectorId = i, DateTime = startDate.AddMinutes(j), Value = r.Next(1, 30) });
                }
            }
            measurements.Sort((m1, m2) => m1.DateTime.CompareTo(m2.DateTime));
            return measurements;
        }

        public static List<Measurement> GetUpdateData(DateTime dateTime)
        {
            var measurements = new List<Measurement>();
            var r = new Random();

            for (int i = 0; i < 5; i++)
            {
                measurements.Add(new Measurement() { DetectorId = i, DateTime = dateTime.AddSeconds(1), Value = r.Next(1, 30) });
            }
            return measurements;
        }
    }

    public class Measurement
    {
        public int DetectorId { get; set; }
        public int Value { get; set; }
        public DateTime DateTime { get; set; }
    }
}
