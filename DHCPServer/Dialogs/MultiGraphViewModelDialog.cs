using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DHCPServer.Dialogs
{
    public class MultiGraphViewModelDialog : DialogViewModelBase
    {
        private List<MultiGraphMouseWheelHandler> _multiGraphMouseWheelHandler = new List<MultiGraphMouseWheelHandler>();
        //	private IEnumerable<LineAnnotation> _annotations = new List<LineAnnotation>();
        //	private int _wheelCount = 0;
        private readonly IMultiRoomRepository _multiRoomRepository;

        private MultiRoomLineGraphInfo _current;

        private MultiRoomLineGraphInfo _graphInfo;

        public MultiRoomLineGraphInfo GraphInfo
        {
            get { return _graphInfo; }
            set { SetProperty(ref _graphInfo, value); }
        }

        private string _labelResult;
        public string LabelResult
        {
            get { return _labelResult; }
            set { SetProperty(ref _labelResult, value); }
        }

        private string _temperature = "Температура";
        public string Temperature
        {
            get { return _temperature; }
            set { SetProperty(ref _temperature, value); }
        }
        private string _humidity = "Влажность";
        public string Humidity
        {
            get { return _humidity; }
            set { SetProperty(ref _humidity, value); }
        }
        public MultiGraphViewModelDialog(IMultiRoomRepository multiRoomRepository)
        {
            _multiRoomRepository = multiRoomRepository;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters != null)
            {
                bool setSetting = true;
                _current = parameters.GetValue<MultiRoomLineGraphInfo>("model");
                GraphInfo = _current;

                var title = $"{_current?.ActiveDevice?.Nick} {_current?.ActiveDevice?.IPAddress}";
                Title = title;
                if (parameters.TryGetValue("date", out DateTimeSpanFilter date))
                {
                    setSetting = false;
                    Task.Run(async () =>
                    {
                        var collection = await _multiRoomRepository.FilterRooms(_current.ActiveDevice.IPAddress, date.FromDate, date.ToDate);
                        GraphInfo = new MultiRoomLineGraphInfo(GraphInfo.ActiveDevice, false);

                        GraphInfo.GraphLineModelForDefault = InitializeDefault(collection, 1);
                        GraphInfo.GraphLineModelForMiddle = InitializeDefault(collection, 2);
                        GraphInfo.GraphLineModelForProcessForNord = InitializeDefault(collection, 3);
                        GraphInfo.GraphLineModelForProcess = InitializeDefault(collection, 4);

                        _multiGraphMouseWheelHandler.Add(new MultiGraphMouseWheelHandler { Model = GraphInfo.GraphLineModelForDefault, Code = 1 });
                        _multiGraphMouseWheelHandler.Add(new MultiGraphMouseWheelHandler { Model = GraphInfo.GraphLineModelForMiddle, Code = 2 });
                        _multiGraphMouseWheelHandler.Add(new MultiGraphMouseWheelHandler { Model = GraphInfo.GraphLineModelForProcessForNord, Code = 3 });
                        _multiGraphMouseWheelHandler.Add(new MultiGraphMouseWheelHandler { Model = GraphInfo.GraphLineModelForProcess, Code = 4 });

                    }).ContinueWith(t =>
                    {
                        var x = t.Exception;
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }

                if (setSetting)
                    SetSettings();

            }
        }
        public ViewResolvingPlotModel InitializeDefault(IEnumerable<MultiRoomInfo> collection, int code)
        {
            ViewResolvingPlotModel result = ViewResolvingPlotModel.CreateDefault();
            var humidityLineSerie = result.GetLast();
            var temperatureLineSerie = result.GetFirst();
            List<DataPoint> humidityPoints = new List<DataPoint>();
            List<DataPoint> temperaturePoints = new List<DataPoint>();

            switch (code)
            {
                case 1:
                    humidityPoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.Humidity)).ToList();
                    temperaturePoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.Temperature)).ToList();
                    break;
                case 2:
                    humidityPoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.HumidityMiddle)).ToList();
                    temperaturePoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.TemperatureMiddle)).ToList();
                    break;
                case 3:
                    humidityPoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.HumidityProcess)).ToList();
                    temperaturePoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.TemperatureNord)).ToList();
                    break;
                case 4:
                    humidityPoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.HumidityProcess)).ToList();
                    temperaturePoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.TemperatureProcess)).ToList();
                    break;
                default:
                    break;
            }


            result.FillCollection(temperatureLineSerie, temperaturePoints);
            result.FillCollection(humidityLineSerie, humidityPoints);

            result.SetLastNHours(6);
            result.AddAnnotationEveryDay();

            //		GraphInfo.GraphLineModelForDefault.InvalidatePlot(true);

            return result;
        }

        private void SetSettings()
        {
            if (GraphInfo?.GraphLineModelForDefault == null) return;

            GraphInfo.GraphLineModelForDefault.Axes[0].Reset();
            GraphInfo.GraphLineModelForDefault.Axes[1].Reset();
            GraphInfo.GraphLineModelForDefault.SetLastNHours(6);
            GraphInfo.GraphLineModelForDefault.AddAnnotationEveryDay();

            GraphInfo.GraphLineModelForMiddle.Axes[0].Reset();
            GraphInfo.GraphLineModelForMiddle.Axes[1].Reset();
            GraphInfo.GraphLineModelForMiddle.SetLastNHours(6);
            GraphInfo.GraphLineModelForMiddle.AddAnnotationEveryDay();

            GraphInfo.GraphLineModelForProcessForNord.Axes[0].Reset();
            GraphInfo.GraphLineModelForProcessForNord.Axes[1].Reset();
            GraphInfo.GraphLineModelForProcessForNord.SetLastNHours(6);
            GraphInfo.GraphLineModelForProcessForNord.AddAnnotationEveryDay();

            GraphInfo.GraphLineModelForProcess.Axes[0].Reset();
            GraphInfo.GraphLineModelForProcess.Axes[1].Reset();
            GraphInfo.GraphLineModelForProcess.SetLastNHours(6);
            GraphInfo.GraphLineModelForProcess.AddAnnotationEveryDay();
        }

        public void LineMouseWheelEventHandler(MouseWheelEventArgs e, int code)
        {
            var model = _multiGraphMouseWheelHandler.FirstOrDefault(x => x.Code == code);
            if (e.Delta < 0)
            {
                model.WheelCount--;
            }
            if (e.Delta >= 0)
            {
                model.WheelCount++;
            }

            var leftAxis = model.Model.Axes[1];
            var rightAxis = model.Model.Axes[0];
            if (model.WheelCount == -2)
            {
                model.Model.ResetAllAxes();
                rightAxis.MajorStep = 1.0 / 24;
            }

            if (model.WheelCount == -6)
            {
                rightAxis.MajorStep = 1.0 / 12;
            }

            if (model.WheelCount == -8)
            {
                rightAxis.MajorStep = 1.0 / 6;
                model.LineAnnotation.Select(x => { x.StrokeThickness = 2; return x; });
            }

            if (model.WheelCount == -15)
            {
                model.LineAnnotation.Select(x => { x.StrokeThickness = 1; return x; });
                rightAxis.MajorStep = 1.0 / 2;
                rightAxis.StringFormat = "HH:mm";
                ((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Hours;
            }
            if (model.WheelCount == -21)
            {
                rightAxis.MajorStep = 1.0;
                rightAxis.StringFormat = "dd/MM/yyy";
                ((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Days;

            }

            if (model.WheelCount == -25)
            {
                rightAxis.MajorStep = 1.5;
                rightAxis.StringFormat = "dd/MM/yyy";
                ((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Days;

            }


            if (model.WheelCount == 8)
            {
                leftAxis.MinimumMajorStep = 3;
            }

            if (model.WheelCount == 9)
            {
                leftAxis.MinimumMajorStep = 2;
            }
            if (model.WheelCount == 13)
            {

                leftAxis.MinimumMajorStep = 1;
            }
            if (model.WheelCount == 17)
            {

                leftAxis.MinimumMajorStep = 0.5;
            }
            if (model.WheelCount == 19)
            {

                leftAxis.MinimumMajorStep = 0.2;
            }
        }

    }

    public class MultiGraphMouseWheelHandler
    {
        private IEnumerable<LineAnnotation> _lineAnnotation;
        public IEnumerable<LineAnnotation> LineAnnotation
        {
            get
            {
                if (_lineAnnotation == null)
                    _lineAnnotation = Model.Annotations.Where(x => x.Tag?.ToString() == "period").Cast<LineAnnotation>();
                return _lineAnnotation;
            }
        }
        public PlotModel Model { get; set; }
        public int WheelCount { get; set; } = 0;
        public int Code { get; set; }

    }
}
