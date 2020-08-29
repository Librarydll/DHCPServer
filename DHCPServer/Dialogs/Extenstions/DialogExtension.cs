using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Dialogs.Extenstions
{
	public static class DialogExtension
	{
		public static void ShowModal(this IDialogService dialogService, string dialogViewName, IDialogParameters dialogParameters, Action<Prism.Services.Dialogs.IDialogResult> callBack)
		{
			dialogService.ShowDialog(dialogViewName, dialogParameters, callBack);
		}
		public static void ShowModal(this IDialogService dialogService, string dialogViewName, Action<IDialogResult> callBack)
		{
			dialogService.ShowDialog(dialogViewName, null, callBack);
		}
	}
}
