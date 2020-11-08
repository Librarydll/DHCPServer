using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace DHCPServer.UICore.Behaviors
{
	public class ActiveButton:Behavior<Button>
	{
        public static BrushConverter converter;
        static ActiveButton()
        {
            converter = new BrushConverter();
            Active = (SolidColorBrush)converter.ConvertFromString("#FF2EA2A2");
            InActive = null;
        }
        public static Brush Active { get; set; }
        public static Brush InActive { get; set; }
        private bool _isActive = false;
        protected override void OnAttached()
        {
            AssociatedObject.Click += OnClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Click -= OnClick;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            //_isActive = !_isActive;
            //var btn = sender as Button;

            //if (_isActive)
            //{
            //    btn.Background = Active;
            //}
            //else
            //{
            //    btn.Background = InActive;
            //}

        }
    }
}
