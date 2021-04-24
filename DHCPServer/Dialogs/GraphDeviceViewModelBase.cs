using DHCPServer.Domain.Interfaces;
using DHCPServer.Models.Infrastructure;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DHCPServer.Dialogs
{
    public class GraphDeviceViewModelBase : DialogViewModelBase
    {
        private bool _dispose = false;
        private int _wheelCount = 0;
        protected IEnumerable<LineAnnotation> _annotations = new List<LineAnnotation>();

        private RoomLineGraphInfo _current;
        public RoomLineGraphInfo GraphInfo
        {
            get { return _current; }
            set { SetProperty(ref _current, value); }
        }

        private RoomLineGraphInfo _temperatureGraphInfo;

        public RoomLineGraphInfo TemperatureGraphInfo
        {
            get { return _temperatureGraphInfo; }
            set { SetProperty(ref _temperatureGraphInfo, value); }
        }

        private RoomLineGraphInfo _humidityGraphInfo;

        public RoomLineGraphInfo HumidityGraphInfo
        {
            get { return _humidityGraphInfo; }
            set { SetProperty(ref _humidityGraphInfo, value); }
        }


        private int _selectedHumidityCombo;
        public int SelectedHumidityCombo
        {
            get { return _selectedHumidityCombo; }
            set
            {
                SetProperty(ref _selectedHumidityCombo, value);
                if (HumidityGraphInfo?.GraphLineModel != null)
                {
                    HumidityGraphInfo.GraphLineModel.Axes[0].Reset();
                    HumidityGraphInfo.GraphLineModel.Axes[1].Reset();
                    HumidityGraphInfo.GraphLineModel.SetLastNHours(GetNumberFromIndex(value));
                    HumidityGraphInfo.GraphLineModel.InvalidatePlot(true);
                }

            }
        }

        private int _selectedTemperatureCombo;
        public int SelectedTemperatureCombo
        {
            get { return _selectedTemperatureCombo; }
            set
            {
                SetProperty(ref _selectedTemperatureCombo, value);
                if (TemperatureGraphInfo?.GraphLineModel != null)
                {
                    TemperatureGraphInfo.GraphLineModel.Axes[0].Reset();
                    TemperatureGraphInfo.GraphLineModel.Axes[1].Reset();
                    TemperatureGraphInfo.GraphLineModel.SetLastNHours(GetNumberFromIndex(value));
                    TemperatureGraphInfo.GraphLineModel.InvalidatePlot(true);
                }
            }
        }

        public int GetNumberFromIndex(int index)
        {
            if (index == 0) return 6;
            if (index == 1) return 12;
            if (index == 2) return 24;
            return 6;
        }


        public virtual void InitializeAnnotations()
        {

        }

        protected void SetSettings()
        {
            if (TemperatureGraphInfo?.GraphLineModel != null)
            {
                TemperatureGraphInfo.GraphLineModel.Axes[0].Reset();
                TemperatureGraphInfo.GraphLineModel.Axes[1].Reset();
                TemperatureGraphInfo.GraphLineModel.SetLastNHours(6);
                TemperatureGraphInfo.GraphLineModel.AddAnnotationEveryDay();
            }

            if (HumidityGraphInfo?.GraphLineModel != null)
            {
                HumidityGraphInfo.GraphLineModel.Axes[0].Reset();
                HumidityGraphInfo.GraphLineModel.Axes[1].Reset();
                HumidityGraphInfo.GraphLineModel.SetLastNHours(6);
                HumidityGraphInfo.GraphLineModel.AddAnnotationEveryDay();
            }

        }

        public void HumidityLineMouseWheelEventHandler(MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                _wheelCount--;
            }
            if (e.Delta >= 0)
            {
                _wheelCount++;
            }

            var leftAxis = HumidityGraphInfo.GraphLineModel.Axes[1];
            var rightAxis = HumidityGraphInfo.GraphLineModel.Axes[0];
            if (_wheelCount == -2)
            {
                HumidityGraphInfo.GraphLineModel.ResetAllAxes();
                rightAxis.MajorStep = 1.0 / 24;
            }

            if (_wheelCount == -6)
            {
                rightAxis.MajorStep = 1.0 / 12;
            }

            if (_wheelCount == -8)
            {
                rightAxis.MajorStep = 1.0 / 6;
                _annotations.Select(x => { x.StrokeThickness = 2; return x; });
            }

            if (_wheelCount == -15)
            {
                _annotations.Select(x => { x.StrokeThickness = 1; return x; });
                rightAxis.MajorStep = 1.0 / 2;
                rightAxis.StringFormat = "HH:mm";
                ((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Hours;
            }
            if (_wheelCount == -21)
            {
                rightAxis.MajorStep = 1.0;
                rightAxis.StringFormat = "dd/MM/yyy";
                ((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Days;

            }

            if (_wheelCount == -25)
            {
                rightAxis.MajorStep = 1.5;
                rightAxis.StringFormat = "dd/MM/yyy";
                ((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Days;

            }


            if (_wheelCount == 8)
            {
                leftAxis.MinimumMajorStep = 3;
            }

            if (_wheelCount == 9)
            {
                leftAxis.MinimumMajorStep = 2;
            }
            if (_wheelCount == 13)
            {

                leftAxis.MinimumMajorStep = 1;
            }
            if (_wheelCount == 17)
            {

                leftAxis.MinimumMajorStep = 0.5;
            }
            if (_wheelCount == 19)
            {

                leftAxis.MinimumMajorStep = 0.2;
            }
        }

        public void TemperatureLineMouseWheelEventHandler(MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                _wheelCount--;
            }
            if (e.Delta >= 0)
            {
                _wheelCount++;
            }

            var leftAxis = TemperatureGraphInfo.GraphLineModel.Axes[1];
            var rightAxis = TemperatureGraphInfo.GraphLineModel.Axes[0];
            if (_wheelCount == -2)
            {
                TemperatureGraphInfo.GraphLineModel.ResetAllAxes();
                rightAxis.MajorStep = 1.0 / 24;
                rightAxis.StringFormat = "HH:mm";
                ((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Hours;
            }

            if (_wheelCount == -6)
            {
                rightAxis.MajorStep = 1.0 / 12;
                rightAxis.StringFormat = "HH:mm";
                ((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Hours;
            }

            if (_wheelCount == -8)
            {
                rightAxis.MajorStep = 1.0 / 6;
                _annotations.Select(x => { x.StrokeThickness = 2; return x; });
                rightAxis.StringFormat = "HH:mm";
                ((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Hours;
            }

            if (_wheelCount == -15)
            {
                _annotations.Select(x => { x.StrokeThickness = 1; return x; });
                rightAxis.MajorStep = 1.0 / 2;
                rightAxis.StringFormat = "HH:mm";
                ((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Hours;
            }
            if (_wheelCount == -21)
            {
                rightAxis.MajorStep = 1.0;
                rightAxis.StringFormat = "dd/MM/yyy";
                ((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Days;

            }

            if (_wheelCount == -25)
            {
                rightAxis.MajorStep = 1.5;
                rightAxis.StringFormat = "dd/MM/yyy";
                ((DateTimeAxis)rightAxis).IntervalType = DateTimeIntervalType.Days;

            }


            if (_wheelCount == 8)
            {
                leftAxis.MinimumMajorStep = 3;
            }

            if (_wheelCount == 9)
            {
                leftAxis.MinimumMajorStep = 2;
            }
            if (_wheelCount == 13)
            {

                leftAxis.MinimumMajorStep = 1;
            }
            if (_wheelCount == 17)
            {

                leftAxis.MinimumMajorStep = 0.5;
            }
            if (_wheelCount == 19)
            {

                leftAxis.MinimumMajorStep = 0.2;
            }
        }

        public override void OnDialogClosed()
        {
            if (_dispose)
            {
                TemperatureGraphInfo?.Dispose();
                HumidityGraphInfo?.Dispose();
                OnClosed();
            }
        }

        public virtual void OnClosed()
        {

        }
    }
}
