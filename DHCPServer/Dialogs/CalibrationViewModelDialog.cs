using DHCPServer.Models.Infrastructure;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Dialogs
{
	public class CalibrationViewModelDialog :DialogViewModelBase
	{

		private RoomLineGraphInfo _roomInfo;
		public RoomLineGraphInfo RoomInfo
		{
			get { return _roomInfo; }
			set { SetProperty(ref _roomInfo, value); }
		}



		public override void OnDialogOpened(IDialogParameters parameters)
		{
			if (parameters != null)
			{
				RoomInfo = parameters.GetValue<RoomLineGraphInfo>("model");
				var title = $"{RoomInfo?.Device?.Nick} {RoomInfo?.Device?.IPAddress}";
				Title = title;
			}
		}

	}
}
