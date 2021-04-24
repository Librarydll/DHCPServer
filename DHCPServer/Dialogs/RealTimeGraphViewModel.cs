using DHCPServer.Domain.Interfaces;
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
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters != null)
            {
                GraphInfo = parameters.GetValue<RoomLineGraphInfo>("model");
                TemperatureGraphInfo = RoomLineGraphInfo.CreateDefault();
                HumidityGraphInfo = RoomLineGraphInfo.CreateDefault();

                TemperatureGraphInfo.GraphLineModel.GetLast().IsVisible = false;
                HumidityGraphInfo.GraphLineModel.GetFirst().IsVisible = false;

                GraphInfo.AddToCollectionEvent += Current_AddToCollectionEvent;
                Title = $"{GraphInfo?.ActiveDevice?.Nick} {GraphInfo?.ActiveDevice?.IPAddress}";
                FillTemperatureGraph();
                FillHumidityGraph();
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

        private void FillTemperatureGraph()
        {
            var points = GraphInfo.GraphLineModel.GetFirst().Points;
            TemperatureGraphInfo.GraphLineModel.GetFirst().Points.AddRange(points);
        }

        private void FillHumidityGraph()
        {
            var points = GraphInfo.GraphLineModel.GetLast().Points;
            HumidityGraphInfo.GraphLineModel.GetLast().Points.AddRange(points);
        }

        private void Current_AddToCollectionEvent(Domain.Models.ActiveDevice arg1, Domain.Models.RoomInfo arg2)
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
