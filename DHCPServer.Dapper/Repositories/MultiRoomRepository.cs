using Dapper;
using Dapper.Contrib.Extensions;
using DHCPServer.Dapper.Context;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Dapper.Repositories
{
	public class MultiRoomRepository : IMultiRoomRepository
	{
		private readonly ApplicationContextFactory _factory;

		public MultiRoomRepository()
		{
			_factory = new ApplicationContextFactory();
		}

        public async Task<IEnumerable<MultiRoomInfo>> FilterRooms(string ipAddress, DateTime from, DateTime to)
        {
			string query = @"SELECT *FROM MultiRoomInfos as r 
							Left join ActiveDevices as d on 
							r.deviceid=d.id where date>=@from and date<=@to and d.ipaddress=@ipAddress";
			using (var connection = _factory.CreateConnection())
			{
				var result = await connection.QueryAsync<MultiRoomInfo, ActiveDevice, MultiRoomInfo>(query,
					(r, d)
					=>
					{
						r.ActiveDevice = d;
						return r;
					},
					new
					{ from = from.Date.ToString("yyyy-MM-dd"), to = to.Date.ToString("yyyy-MM-dd"), ipAddress });
				return result;
			}
		}

        public async Task<MultiRoomInfo> SaveAsync(MultiRoomInfo roomInfo)
		{
			using (var connection = _factory.CreateConnection())
			{
				roomInfo.Id = await connection.InsertAsync(roomInfo);
				return roomInfo;
			}
		}
	}
}
