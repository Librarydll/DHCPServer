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
