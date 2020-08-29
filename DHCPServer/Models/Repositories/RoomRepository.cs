using Dapper;
using Dapper.Contrib.Extensions;
using DHCPServer.Models.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.Repositories
{
	public class RoomRepository : IRoomRepository
	{
		private readonly ApplicationContextFactory _factory;
		public RoomRepository()
		{
			_factory = new ApplicationContextFactory();
		}


		public async Task<RoomInfo> SaveAsync(RoomInfo roomInfo)
		{
			using (var connection = _factory.CreateConnection())
			{
				roomInfo.Id = await connection.InsertAsync(roomInfo);
				return roomInfo;
			}
		}
		public async Task SaveAsync(IEnumerable<RoomInfo> roomInfos)
		{
			using (var connection = _factory.CreateConnection())
			{
				foreach (var room in roomInfos)
				{
					room.Id = await connection.InsertAsync(room);
				}
			}
		}

		public async Task<IEnumerable<RoomInfo>> GetGroupedRooms()
		{
			//string query = @"SELECT *FROM RoomInfos";
			using (var connection = _factory.CreateConnection())
			{
				var entites = await connection.GetAllAsync<RoomInfo>();


				return entites;
			}
		}

	}
}
