using Dapper;
using Dapper.Contrib.Extensions;
using DHCPServer.Models.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.Repositories
{
	public class DeviceRepository : IDeviceRepository
	{
		private readonly ApplicationContextFactory _factory;

		public DeviceRepository()
		{
			_factory = new ApplicationContextFactory();
		}
		public async Task<Device> CreateAsync(Device device)
		{
			string query = "SELECT *FROM DEVICES WHERE ipaddress =@address";
			using (var connection = _factory.CreateConnection())
			{
				var dev = await connection.QueryFirstOrDefaultAsync<Device>(query, new { address = device.IPAddress });

				if (dev != null) return dev;


				int id = await connection.InsertAsync(device);
				device.Id = id;
				return device;
			}
		}

		public async Task<bool> UpdateAsync(Device device)
		{
			using (var connection = _factory.CreateConnection())
			{
				var isUpdated = await connection.UpdateAsync(device);
				return isUpdated;
			}
		}

		public async Task<bool> DeleteAsync(Device device)
		{
			using (var connection = _factory.CreateConnection())
			{
				var isDeleted = await connection.DeleteAsync(device);
				return isDeleted;
			}
		}

		public async Task<IEnumerable<Device>> GetAllAsync()
		{
			using (var connection = _factory.CreateConnection())
			{
				var entities = await connection.GetAllAsync<Device>();
				return entities;
			}
		}

	}
}
