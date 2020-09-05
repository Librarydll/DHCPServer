using DHCPServer.Models.Infrastructure;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Dialogs
{
	public class GraphViewModelDialog:DialogViewModelBase
	{

		private RoomLineGraphInfo _graphInfo;
		public RoomLineGraphInfo GraphInfo
		{
			get { return _graphInfo; }
			set { SetProperty(ref _graphInfo, value); }
		}

		public DelegateCommand XCommand { get; set; }

		public GraphViewModelDialog()
		{
			XCommand = new DelegateCommand(() =>
			 {
				 GraphInfo.GraphLineModel.InvalidatePlot(true);
			 }
			);
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
