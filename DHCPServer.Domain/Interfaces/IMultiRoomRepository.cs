using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Domain.Interfaces
{
	public interface IMultiRoomRepository
	{
		Task<MultiRoomInfo> SaveAsync(MultiRoomInfo roomInfo);

		Task<IEnumerable<MultiRoomInfo>> FilterRooms(string ipAddress, DateTime from, DateTime to);
	}
}
