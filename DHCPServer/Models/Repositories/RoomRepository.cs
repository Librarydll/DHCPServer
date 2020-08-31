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

		public async Task<IEnumerable<RoomInfo>> FilterRooms(DateTime from, DateTime to)
		{
			string query = "SELECT *FROM RoomInfos where date>=@from and date<=@to";
			using (var connection = _factory.CreateConnection())
			{
				var result = await connection.QueryAsync<RoomInfo>(query, new 
				{ from = from.Date.ToString("yyyy-MM-dd"), to = to.Date.ToString("yyyy-MM-dd") });
				return result;
			}
		}

		public async Task<IEnumerable<RoomInfo>> FilterRooms(DateTime fromDate, DateTime toDate, TimeSpan fromTime, TimeSpan toTime)
		{
			string query = "SELECT *FROM RoomInfos where date>=@from and date<=@to";

			using (var connection = _factory.CreateConnection())
			{
				var fd = new DateTime(fromDate.Year,fromDate.Month,fromDate.Day,fromTime.Hours,fromTime.Minutes,0);
				var ft = new DateTime(toDate.Year, toDate.Month, toDate.Day, toTime.Hours, toTime.Minutes,0);

				var result = await connection.QueryAsync<RoomInfo>(query, new
				{ from = fd, to = ft });
				return result;
			}
		}
	}
}
