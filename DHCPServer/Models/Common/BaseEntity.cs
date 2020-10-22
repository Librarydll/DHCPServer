using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.Common
{
	public class BaseEntity: INotifyPropertyChanged
	{
		public int Id { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public virtual void RaisePropertyChangedEvent([CallerMemberName]string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}


		public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{

			if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

			storage = value;
			RaisePropertyChangedEvent(propertyName);

			return true;

		}
	}
}
