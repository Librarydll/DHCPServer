﻿using DHCPServer.Models;
using DHCPServer.Models.Infrastructure;
using DHCPServer.Models.Repositories;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.ViewModels
{
	public class GraphDataViewModel:BindableBase
	{
		private readonly IRoomRepository _roomRepository;

		#region Properties
		private DateTimeSpanFilter _dateTimeSpan =new DateTimeSpanFilter();

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
			FilterCommand = new DelegateCommand(Filter);
			RoomInfoColleciton = new ObservableCollection<RoomInfo>();
			_roomRepository = roomRepository;
		}

		private void Filter()
		{
			Task.Run(async () =>
			{
				IEnumerable<RoomInfo> collection = null;

				if (DateTimeSpan.IsTimeInclude&& DateTimeSpan.Validate())
				{
					collection = await _roomRepository.FilterRooms(DateTimeSpan.FromDate, DateTimeSpan.ToDate, DateTimeSpan.FromTime, DateTimeSpan.ToTime);
				}
				else if(DateTimeSpan.IsDateValidate())
				{
					collection = await _roomRepository.FilterRooms(DateTimeSpan.FromDate, DateTimeSpan.ToDate);
				}
				if(collection!=null)
					RoomInfoColleciton = new ObservableCollection<RoomInfo>(collection);
				else
				{
					RoomInfoColleciton.Clear();
				}
			});
			

		}
	}
}
