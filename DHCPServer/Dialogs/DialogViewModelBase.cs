using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Dialogs
{
	public class DialogViewModelBase : BindableBase, IDialogAware
	{
		public event Action<IDialogResult> RequestClose;

		public ButtonResult Result { get; set; }
		public string Title => string.Empty;
		public DelegateCommand<IDialogParameters> CloseDialogCancelCommand =>
			new DelegateCommand<IDialogParameters>(CloseDialogOnCancel);

		public DelegateCommand<IDialogParameters> CloseDialogOkCommand =>
			new DelegateCommand<IDialogParameters>(CloseDialogOnOk);

		//public virtual DelegateCommand<KeyEventArgs> KeyUpEventCommand { get; protected set; }


		public DialogViewModelBase()
		{
			//KeyUpEventCommand = new DelegateCommand<KeyEventArgs>(KeyUpEventHandler);
		}
		//protected virtual void KeyUpEventHandler(KeyEventArgs key)
		//{
		//	if (key.Key == Key.Escape)
		//	{
		//		CloseDialogOnCancel(null);
		//	}
		//	if (key.Key == Key.Enter)
		//	{
		//		CloseDialogOnOk(null);
		//	}
		//}

		protected virtual void CloseDialogOnCancel(IDialogParameters parameters)
		{
			Result = ButtonResult.Cancel;

			CloseDialog(parameters);
		}

		protected virtual void CloseDialogOnOk(IDialogParameters parameters)
		{
			Result = ButtonResult.OK;

			CloseDialog(parameters);
		}


		public bool CanCloseDialog()
		{
			return true;
		}

		public void OnDialogClosed()
		{
		}

		public void CloseDialog(IDialogParameters parameters)
		{
			RequestClose?.Invoke(new DialogResult(Result, parameters));
		}

		public virtual void OnDialogOpened(IDialogParameters parameters)
		{

		}
	}
}
