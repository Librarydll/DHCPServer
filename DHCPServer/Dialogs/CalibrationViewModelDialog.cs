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
		private RoomLineGraphInfoSetting _roomInfo;
		public RoomLineGraphInfoSetting RoomInfo
		{
			get { return _roomInfo; }
			set { SetProperty(ref _roomInfo, value); }
		}

		public CalibrationViewModelDialog()
		{
			RoomInfo = new RoomLineGraphInfoSetting();
		}


		public override void OnDialogOpened(IDialogParameters parameters)
		{
			if (parameters != null)
			{
				var room = parameters.GetValue<RoomLineGraphInfo>("model");
				RoomInfo.TemperatureRange = room.Setting.TemperatureRange;
				RoomInfo.HumidityRange = room.Setting.HumidityRange;
				var title = $"{room?.ActiveDevice?.Device?.Nick} {room?.ActiveDevice?.Device?.IPAddress}";
				Title = title;
			}
		}

		protected override void CloseDialogOnOk(IDialogParameters parameters)
		{
			Result = ButtonResult.OK;
			parameters = new DialogParameters();
			parameters.Add("model", RoomInfo);
			CloseDialog(parameters);

		}

	}
}
