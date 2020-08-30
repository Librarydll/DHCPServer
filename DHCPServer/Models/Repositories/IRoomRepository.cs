using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.Repositories
{
	public interface IRoomRepository
	{
		Task<RoomInfo> SaveAsync(RoomInfo roomInfo);
		Task SaveAsync(IEnumerable<RoomInfo> roomInfos);
	}
}
