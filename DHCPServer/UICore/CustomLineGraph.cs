using Microsoft.Research.DynamicDataDisplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DHCPServer.UICore
{
	class CustomLineGraph:LineGraph
	{
        public static readonly DependencyProperty LineHeaderProperty =
        DependencyProperty.Register("LineHeader", typeof(string), typeof(CustomLineGraph), new
           PropertyMetadata("", new PropertyChangedCallback(OnLineHeaderChanged)));

        public string LineHeader
        {
            get { return (string)GetValue(LineHeaderProperty); }
            set { SetValue(LineHeaderProperty, value); }
        }

        public Brush LineGraphBrush
        {
            get { return (Brush)GetValue(LineGraphBrushProperty); }
            set { SetValue(LineGraphBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineGraphBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineGraphBrushProperty =
            DependencyProperty.Register("LineGraphBrush", typeof(Brush), typeof(CustomLineGraph), new PropertyMetadata(Brushes.Black,new PropertyChangedCallback(OnLineBrushChanged)));

        private static void OnLineBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomLineGraph clg = d as CustomLineGraph;
            clg.OnLineBrushChanged(e);
        }

        private static void OnLineHeaderChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            CustomLineGraph clg = d as CustomLineGraph;
            clg.OnLineHeaderChanged(e);
        }

        private void OnLineHeaderChanged(DependencyPropertyChangedEventArgs e)
        {
            Description = new PenDescription(e.NewValue.ToString());
        }
        private void OnLineBrushChanged(DependencyPropertyChangedEventArgs e)
        {
            LinePen = new Pen((Brush)e.NewValue, LinePen.Thickness);
        }
    }
}
