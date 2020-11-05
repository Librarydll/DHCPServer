using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.Infrastructure
{
    public class ViewResolvingPlotModel : PlotModel, IPlotModel
    {
        private static readonly OxyColor temperatureColor = OxyColors.Red;
        private static readonly OxyColor humidityColor = OxyColors.Blue;
        private static readonly MarkerType markerType = MarkerType.None;
        private static readonly Type BaseType = typeof(ViewResolvingPlotModel).BaseType;
        private static readonly MethodInfo BaseAttachMethod = BaseType
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(methodInfo => methodInfo.IsFinal && methodInfo.IsPrivate)
            .FirstOrDefault(methodInfo => methodInfo.Name.EndsWith(nameof(IPlotModel.AttachPlotView)));

        void IPlotModel.AttachPlotView(IPlotView plotView)
        {
            //because of issue https://github.com/oxyplot/oxyplot/issues/497 
            //only one view can ever be attached to one plotmodel
            //we have to force detach previous view and then attach new one
            if (plotView != null && PlotView != null && !Equals(plotView, PlotView))
            {
                BaseAttachMethod.Invoke(this, new object[] { null });
                BaseAttachMethod.Invoke(this, new object[] { plotView });
            }
            else
            {
                BaseAttachMethod.Invoke(this, new object[] { plotView });
            }
        }

		public static ViewResolvingPlotModel CreateDefault()
		{
			var model = new ViewResolvingPlotModel();
			SetUpModel(model);
			var lineSeries = CreateLineSeries();
			model.Series.Add(lineSeries.First());
			model.Series.Add(lineSeries.Last());

			return model;
		}

		public LineSeries GetFirst()
		{
			return Series.First() as LineSeries;
		}
		public LineSeries GetLast()
		{
			return Series.Last() as LineSeries;

		}
	

		private static LineSeries[] CreateLineSeries()
		{
			var temperatureLineSeries = new LineSeries
			{
				StrokeThickness = 2,
				MarkerSize = 3,
				MarkerStroke = temperatureColor,
				MarkerType = markerType,
				CanTrackerInterpolatePoints = true,
				Title = "Температура",
				Color = temperatureColor,

				
			};			
			var humidityLineSeries = new LineSeries
			{
				StrokeThickness = 2,
				MarkerSize = 3,
				MarkerStroke = humidityColor,
				MarkerType = markerType,
				CanTrackerInterpolatePoints = true,
				Title = "Влажность",
				Color = humidityColor,
			};

			return new LineSeries[]
			{
				temperatureLineSeries,humidityLineSeries
			};
		}
		private static void SetUpModel(ViewResolvingPlotModel model)
		{
			model.LegendTitle = "Данные";
			model.LegendOrientation = LegendOrientation.Horizontal;
			model.LegendPlacement = LegendPlacement.Outside;
			model.LegendPosition = LegendPosition.TopRight;
			model.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
			model.LegendBorder = OxyColors.Black;
			var dt = DateTime.Now;
			var dateAxis = new DateTimeAxis()
			{
				MajorGridlineStyle = LineStyle.Solid,
				MinorGridlineStyle = LineStyle.Dot,
				IntervalLength = 200,
				Position = AxisPosition.Bottom,
				Title = "Время",
				MajorStep = 1.0 / 24,
				StringFormat = "HH:mm",
				Minimum = DateTimeAxis.ToDouble(dt),
				Maximum = DateTimeAxis.ToDouble(dt.AddHours(6)),
				IntervalType = DateTimeIntervalType.Hours,
				
				
			};
			model.Axes.Add(dateAxis);
			var valueAxis = new LinearAxis()
			{
				MajorGridlineStyle = LineStyle.Solid,
				MinorGridlineStyle = LineStyle.Dot,
				Title = "Значение",
				Position = AxisPosition.Left,
				MinorStep = 0.5,
				MajorStep = 0.2,
				MinimumMinorStep = 0.5,
				MinimumMajorStep = 3,
				IntervalLength = 100,
				AbsoluteMaximum = 70,
				AbsoluteMinimum = 0,
			};
			model.Axes.Add(valueAxis);
		}


		public bool SetLastNHours(int n)
		{
			var dateTimeAxis = Axes.First() as DateTimeAxis;
			var lineSeries = GetFirst();
			var dates = lineSeries.Points.Select(x =>DateTimeAxis.ToDateTime(x.X));
			var beginingDate = dates.FirstOrDefault();
			var lastDate = dates.LastOrDefault();

			if (beginingDate == null || lastDate == null) return false;

			var subsctractDate = lastDate - beginingDate;

			if (Math.Abs(subsctractDate.Hours) <= n) return false;

			var lastNDate = lastDate.Subtract(new TimeSpan(n, 0, 0));

			dateTimeAxis.Minimum = DateTimeAxis.ToDouble(lastNDate);
			dateTimeAxis.Maximum = DateTimeAxis.ToDouble(lastDate);

			return true;
		}


		public void AddAnnotations(int n)
		{
			var points = GetFirst().Points;

			var firstPoint = points.Min(x => x.X);
			var lastPoint = points.Max(x=>x.X);
			foreach (var item in points)
			{
				Console.WriteLine(DateTimeAxis.ToDateTime(item.X));
			}
			var beginingDate = DateTimeAxis.ToDateTime(lastPoint);
			var lastDate = DateTimeAxis.ToDateTime(firstPoint);
			double Y = 70;
			double X = DateTimeAxis.ToDouble(lastDate);
			var hours =(int)(lastDate - beginingDate).Hours;

			for (int i = 0; i < hours/n; i++)
			{
				LineAnnotation Line = new LineAnnotation()
				{
					StrokeThickness = 1,
					Color = OxyColors.Green,
					Type = LineAnnotationType.Vertical,
					Text = (Y).ToString(),
					TextColor = OxyColors.White,
					X = X,
					Y = Y,
					LineStyle = LineStyle.Solid
				};
				Annotations.Add(Line);
				X= DateTimeAxis.ToDouble(lastDate.AddHours(n));
			}


			
		}
	}
}
