﻿using OxyPlot;
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

		public static ViewResolvingPlotModel CreateDefault(double min=0,double max=70)
		{
			var model = new ViewResolvingPlotModel();
			SetUpModel(model,min,max);
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
		private static void SetUpModel(ViewResolvingPlotModel model,double min,double max)
		{
			model.IsLegendVisible = false;
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
				AbsoluteMaximum = max,
				AbsoluteMinimum = min,
			};
			model.Axes.Add(valueAxis);
		}


		public bool SetLastNHours(int n)
		{
			var dateTimeAxis = Axes.First() as DateTimeAxis;
			var lineSeries = GetFirst();
            if (lineSeries.Points.Count == 0)
            {
				lineSeries = GetLast();
            }
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

		public void AddDataPoint(LineSeries line, DataPoint dataPoints)
        {
			FillCollection(line, new List<DataPoint> { dataPoints });
        }

		public void FillCollection(LineSeries line,IEnumerable<DataPoint> dataPoints)
		{
			double previousY = 20;
			int i = 1;
			foreach (var point in dataPoints)
			{
				bool twoPointIsFarAway = false;
				if (i != dataPoints.Count())
				{
					twoPointIsFarAway = AreTwoPointsFarAway(point.X, dataPoints.ElementAt(i).X);
					i++;
				}
				if (point.Y != 0)
				{
					previousY = point.Y;
				}
				if (point.Y == 0)
				{
					var dot = new PointAnnotation 
					{ 
						X =point.X,
						Y=previousY,
						ToolTip="0",
						Fill = OxyColors.DarkOrange,
						Shape= MarkerType.Circle
					};

					Annotations.Add(dot);
				}
				else
				{
					if (twoPointIsFarAway)
					{
						line.Points.Add(DataPoint.Undefined);
					}
					else
					{
						line.Points.Add(point);
					}

				}
			}

		}

        public void AddAnnotations(int n)
		{
            var points = GetFirst().Points;
			if (points.Count == 0) return;
            var lastPoint = points.Max(x => x.X);
			var firstPoint = points.Min(x => x.X);

			var beginingDate = DateTimeAxis.ToDateTime(firstPoint);

            var lastDate = DateTimeAxis.ToDateTime(lastPoint);
            double Y = 70;
			var hours =(int)(lastDate - beginingDate).TotalHours;
			var date = beginingDate;
			double X = DateTimeAxis.ToDouble(date);
			for (int i = 0; i < hours/n; i++)
			{
				LineAnnotation Line = new LineAnnotation()
				{
					Tag="period",
					StrokeThickness = 2,
					Color = OxyColors.Green,
					Type = LineAnnotationType.Vertical,
					Text = Y.ToString(),
					TextColor = OxyColors.White,
					X = X,
					LineStyle = LineStyle.Dash,

				};
				Annotations.Add(Line);
				date = date.AddHours(n);
				X = DateTimeAxis.ToDouble(date);
			}


			
		}
		public void AddAnnotationEveryDay()
		{
			var points = GetFirst().Points;
			if (points.Count == 0) 
			{
				points = GetLast().Points;
				if (points.Count == 0)
					return;
			}
			var lastPoint = points.Where(x=>!x.Equals(DataPoint.Undefined)).Max(x => x.X );
			var firstPoint = points.Where(x => !x.Equals(DataPoint.Undefined)).Min(x => x.X );

			var beginingDate = DateTimeAxis.ToDateTime(firstPoint);

			var lastDate = DateTimeAxis.ToDateTime(lastPoint);


			double Y = 70;
			var date = new DateTime(beginingDate.Date.Year, beginingDate.Date.Month, beginingDate.Date.Day);
			var date2 = new DateTime(lastDate.Date.Year, lastDate.Date.Month, lastDate.Date.Day);
			var days = (int)(date2 - date).TotalDays;

			double X = DateTimeAxis.ToDouble(date);

			for (int i = 0; i <= days+1 ; i++)
			{
				LineAnnotation Line = new LineAnnotation()
				{
					Tag = "period",
					StrokeThickness = 2,
					Color = OxyColors.Green,
					Type = LineAnnotationType.Vertical,
					Text = Y.ToString(),
					TextColor = OxyColors.White,
					X = X,
					LineStyle = LineStyle.Dash,

				};
				Annotations.Add(Line);
				date = date.AddDays(1);
				X = DateTimeAxis.ToDouble(date);
			}

		}

		public bool AreTwoPointsFarAway(double previous, double next)
		{
			var prev = DateTimeAxis.ToDateTime(previous);
			var n = DateTimeAxis.ToDateTime(next);

			var difference = n - prev;
			if (difference.Minutes > 11)
			{
				return true;
			}

			return false;
		}
	}
}
