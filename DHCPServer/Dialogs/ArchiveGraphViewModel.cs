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

namespace DHCPServer.Dialogs
{
    public class ArchiveGraphViewModel : GraphDeviceViewModelBase
    {
        private readonly IRoomRepository _roomRepository;
        private DateTimeSpanFilter _dateTimeSpan = new DateTimeSpanFilter();

        public DateTimeSpanFilter DateTimeSpan
        {
            get { return _dateTimeSpan; }
            set { SetProperty(ref _dateTimeSpan, value); }
        }

        public ArchiveGraphViewModel(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {

            if (parameters != null)
            {
                GraphInfo = parameters.GetValue<RoomLineGraphInfo>("model");

                if (parameters.TryGetValue("id", out int reportId))
                {
                    TemperatureGraphInfo = RoomLineGraphInfo.CreateDefault();
                    HumidityGraphInfo = RoomLineGraphInfo.CreateDefault();
                    Task.Run(async () =>
                    {
                        var collection = await _roomRepository.FilterRooms(reportId,GraphInfo.ActiveDevice.Id);
                        if (collection.Count() > 0) 
                        {
                            TemperatureGraphInfo = FillTemperatureModel(collection);
                            HumidityGraphInfo = FillHumidityModel(collection);
                        }
                        
                    });
                }

                Title = $"{GraphInfo?.ActiveDevice?.Nick} {GraphInfo?.ActiveDevice?.IPAddress}";
                InitializeAnnotations();
                SetSettings();
            }
        }

        public RoomLineGraphInfo FillHumidityModel(IEnumerable<RoomInfo> collection)
        {
            var result = RoomLineGraphInfo.CreateDefault();
            var lineAxis = result.GraphLineModel.Axes.FirstOrDefault() as LinearAxis;
            lineAxis.AbsoluteMinimum = 45;
            lineAxis.AbsoluteMaximum = 75;

            var humidityLineSerie = result.GraphLineModel.GetLast();
            var humidityPoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.Humidity)).ToList();
            var min = collection.Min(x => x.Date);
            result.GraphLineModel.FillCollection(humidityLineSerie, humidityPoints);

            result.GraphLineModel.SetLastNHours(6);
            result.GraphLineModel.AddAnnotationEveryDay();

            return result;
        }
        public RoomLineGraphInfo FillTemperatureModel(IEnumerable<RoomInfo> collection)
        {

            var result = RoomLineGraphInfo.CreateDefault();
            var lineAxis =result.GraphLineModel.Axes.FirstOrDefault() as LinearAxis;
            lineAxis.AbsoluteMinimum = 27;
            lineAxis.AbsoluteMaximum = 60;
            var temperatureLineSerie = result.GraphLineModel.GetFirst();
            var temperaturePoints = collection.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.Temperature)).ToList();
            var min = collection.Min(x => x.Date);
            result.GraphLineModel.FillCollection(temperatureLineSerie, temperaturePoints);

            result.GraphLineModel.SetLastNHours(6);
            result.GraphLineModel.AddAnnotationEveryDay();

            return result;
        }

        public override void InitializeAnnotations()
        {
            _annotations = TemperatureGraphInfo?.GraphLineModel.Annotations
                                                          .Where(x => x.Tag?.ToString() == "period")
                                                          .Cast<LineAnnotation>();
        }
    }
}
