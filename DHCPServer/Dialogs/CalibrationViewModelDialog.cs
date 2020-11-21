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
		private string _temperature;
		public string Temperature
		{
			get { return _temperature; }
			set { SetProperty(ref _temperature, value); }
		}

		private string _humidity;
		public string Humidity
		{
			get { return _humidity; }
			set { SetProperty(ref _humidity, value); }
		}
		public override void OnDialogOpened(IDialogParameters parameters)
		{
			if (parameters != null)
			{
				var room = parameters.GetValue<RoomLineGraphInfo>("model");
				Temperature = room.Setting.TemperatureRange.ToString();
				Humidity = room.Setting.HumidityRange.ToString();
				var title = $"{room?.ActiveDevice?.Nick} {room?.ActiveDevice?.IPAddress}";
				Title = title;
			}
		}

		protected override void CloseDialogOnOk(IDialogParameters parameters)
		{
			var room = new RoomLineGraphInfoSetting();
			if(double.TryParse(Temperature.Replace(".",","),out double t))
            {}
			if (double.TryParse(Humidity.Replace(".", ","), out double h))
			{}
			room.SetSetting(t, h);
			Result = ButtonResult.OK;
			parameters = new DialogParameters();
			parameters.Add("model", room);
			CloseDialog(parameters);

		}

	
	}
}
