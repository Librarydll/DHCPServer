using DHCPServer.Models.Infrastructure;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DHCPServer.Dialogs
{
	public class GraphViewModelDialog:DialogViewModelBase
	{
		private int _wheelCount = 0;

		private RoomLineGraphInfo _graphInfo;
		public RoomLineGraphInfo GraphInfo
		{
			get { return _graphInfo; }
			set { SetProperty(ref _graphInfo, value); }
		}



		public void LineMouseWheelEventHandler(MouseWheelEventArgs e)
		{
			if(e.Delta < 0)
			{
				_wheelCount--;
			}
			if (e.Delta >= 0)
			{
				_wheelCount++;
			}

			var axis = GraphInfo.GraphLineModel.Axes[1];
			if(_wheelCount == 8)
			{
				axis.MinimumMajorStep = 3;
			}

			if (_wheelCount == 9)
			{
				axis.MinimumMajorStep = 2;
			}
			if (_wheelCount == 13)
			{

				axis.MinimumMajorStep = 1;
			}
			if (_wheelCount == 17)
			{

				axis.MinimumMajorStep = 0.5;
			}
			if (_wheelCount == 19)
			{

				axis.MinimumMajorStep = 0.2;
			}
		}

		public override void OnDialogOpened(IDialogParameters parameters)
		{
			if (parameters != null)
			{
				GraphInfo = parameters.GetValue<RoomLineGraphInfo>("model");
				GraphInfo.GraphLineModel.Axes[0].Reset();
				GraphInfo.GraphLineModel.Axes[1].Reset();
			}
		}
	}
}
