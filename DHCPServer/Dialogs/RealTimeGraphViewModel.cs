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
    public class RealTimeGraphViewModel : GraphDeviceViewModelBase
    {
        private readonly IRoomRepository _roomRepository;

        private string _temperatureText ="Температура";
        public string TemperatureText
        {
            get { return _temperatureText; }
            set { SetProperty(ref _temperatureText, value); }
        }
        private string _humidityText = "Влажность";

        public string HumidityText
        {
            get { return _humidityText; }
            set { SetProperty(ref _humidityText, value); }
        }

        public RealTimeGraphViewModel(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters != null)
            {
                GraphInfo = parameters.GetValue<RoomLineGraphInfo>("model");
                TemperatureGraphInfo = RoomLineGraphInfo.CreateDefault();
                TemperatureGraphInfo.GraphLineModel = ViewResolvingPlotModel.CreateDefault(18, 60);
                HumidityGraphInfo = RoomLineGraphInfo.CreateDefault();
                HumidityGraphInfo.GraphLineModel = ViewResolvingPlotModel.CreateDefault(45, 75);

                TemperatureGraphInfo.GraphLineModel.GetLast().IsVisible = false;
                HumidityGraphInfo.GraphLineModel.GetFirst().IsVisible = false;

                GraphInfo.AddToCollectionEvent += Current_AddToCollectionEvent;
                Title = $"{GraphInfo?.ActiveDevice?.Nick} {GraphInfo?.ActiveDevice?.IPAddress}";
                var points = _roomRepository.FilterRooms(GraphInfo.ActiveDevice.Id).GetAwaiter().GetResult();
                FillTemperatureGraph(points);
                FillHumidityGraph(points);
                InitializeAnnotations();
                SetSettings();

                if(parameters.TryGetValue("dataType",out int value))
                {
                    if (value == 2) TemperatureText += " Мид";
                    if (value == 3) TemperatureText += " Норд";
                    if (value == 4) TemperatureText += " Процесс";
                }
            }
        }

        private void FillTemperatureGraph(IEnumerable<RoomInfo> roomInfos)
        {
            var temperaturePoints = roomInfos.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.Temperature)).ToList();

            TemperatureGraphInfo.GraphLineModel.FillCollection(TemperatureGraphInfo.GraphLineModel.GetFirst(),temperaturePoints);

            TemperatureGraphInfo.GraphLineModel.InvalidatePlot(true);

        }

        private void FillHumidityGraph(IEnumerable<RoomInfo> roomInfos)
        {
            var humidityPoints = roomInfos.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Date), x.Humidity)).ToList();

            HumidityGraphInfo.GraphLineModel.FillCollection(HumidityGraphInfo.GraphLineModel.GetLast(), humidityPoints);

            HumidityGraphInfo.GraphLineModel.InvalidatePlot(true);
        }

        private void Current_AddToCollectionEvent(ActiveDevice arg1, RoomInfo arg2)
        {
            TemperatureGraphInfo.GraphLineModel.AddDataPoint(TemperatureGraphInfo.GraphLineModel.GetFirst(), new DataPoint(DateTimeAxis.ToDouble(arg2.Date.AddMinutes(10)), arg2.Temperature));
            HumidityGraphInfo.GraphLineModel.AddDataPoint(HumidityGraphInfo.GraphLineModel.GetLast(), new DataPoint(DateTimeAxis.ToDouble(arg2.Date.AddMinutes(10)), arg2.Humidity));

            TemperatureGraphInfo.GraphLineModel.InvalidatePlot(true);
            HumidityGraphInfo.GraphLineModel.InvalidatePlot(true);

        }

        public override void InitializeAnnotations()
        {
            _annotations = GraphInfo?.GraphLineModel.Annotations
                                                          .Where(x => x.Tag?.ToString() == "period")
                                                          .Cast<LineAnnotation>();
        }

        public override void OnClosed()
        {
            GraphInfo.AddToCollectionEvent -= Current_AddToCollectionEvent;
        }
    }
}
