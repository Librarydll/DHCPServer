using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DHCPServer.ViewModels
{
	public class GraphDataViewModel : BindableBase
	{
		private readonly IRoomRepository _roomRepository;

		#region Properties
		private DateTimeSpanFilter _dateTimeSpan = new DateTimeSpanFilter();

		public DateTimeSpanFilter DateTimeSpan
		{
			get { return _dateTimeSpan; }
			set { SetProperty(ref _dateTimeSpan, value); }
		}

		private ObservableCollection<RoomInfo> _roomInfoColleciton;
		public ObservableCollection<RoomInfo> RoomInfoColleciton
		{
			get { return _roomInfoColleciton; }
			set { SetProperty(ref _roomInfoColleciton, value); }
		}
		#endregion

		#region Command
		public DelegateCommand FilterCommand { get; set; }
		#endregion

		public GraphDataViewModel(IRoomRepository roomRepository)
		{
			FilterCommand = new DelegateCommand(async () => await Filter());
			RoomInfoColleciton = new ObservableCollection<RoomInfo>();
			_roomRepository = roomRepository;
		}

		private async Task Filter()
		{
			IEnumerable<RoomInfo> collection = null;

			if (DateTimeSpan.IsTimeInclude && DateTimeSpan.Validate())
			{
				collection = await _roomRepository.FilterRooms(DateTimeSpan.FromDate, DateTimeSpan.ToDate, DateTimeSpan.FromTime, DateTimeSpan.ToTime);
			}
			else if (DateTimeSpan.IsDateValidate())
			{
				collection = await _roomRepository.FilterRooms(DateTimeSpan.FromDate, DateTimeSpan.ToDate);
			}
			if (collection != null)
				RoomInfoColleciton = new ObservableCollection<RoomInfo>(collection);
			else
			{
				RoomInfoColleciton.Clear();
			}

		}
	}
}
