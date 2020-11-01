using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Domain.Interfaces
{
	public interface IRoomRepository
	{
		Task<RoomInfo> SaveAsync(RoomInfo roomInfo);
		Task SaveAsync(IEnumerable<RoomInfo> roomInfos);

		Task<IEnumerable<RoomInfo>> FilterRooms(DateTime from, DateTime to);
		Task<IEnumerable<RoomInfo>> FilterRooms(DateTime fromDate, DateTime toDate,TimeSpan fromTime,TimeSpan toTime);
		Task<IEnumerable<RoomInfo>> FilterRooms(int deviceid, DateTime date, TimeSpan fromTime, TimeSpan toTime);
		Task<IEnumerable<RoomInfo>> FilterRooms(int deviceid, DateTime date);

	}
}
