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

		public async Task<IEnumerable<Device>> GetDevicesLists()
		{
			string query = @"SELECT *FROM DevicesLists as dl left 
							join devices as d on dl.deviceid=d.id";
			using (var connection = _factory.CreateConnection())
			{
				var entities = await connection.QueryAsync<DevicesList, Device, Device>(query,
					(dl, d) =>
					{
						return d;
					});
				return entities;
			}
		}

		public async Task<DevicesList> CreateDeviceListAsync(DevicesList device)
		{
			string query = "SELECT *FROM DevicesLists WHERE deviceid = @id";
			using (var connection = _factory.CreateConnection())
			{
				var dev = await connection.QueryFirstOrDefaultAsync<DevicesList>(query, new { id = device.DeviceId });

				if (dev != null) return dev;


				int id = await connection.InsertAsync(device);
				device.Id = id;
				return device;
			}
		}

	
		public async Task<bool> DeleteDeviceListAsync(int deviceId)
		{
			string query = "Select *from deviceslists where DEVICEID=@deviceId";
			var sqlStatement = "DELETE from deviceslists WHERE Id = @Id";
			using (var connection = _factory.CreateConnection())
			{
				var device = await connection.QueryFirstOrDefaultAsync<DevicesList>(query,new {deviceId });
				var count = await connection.ExecuteAsync(sqlStatement, new { Id = device.Id });
				return count>0;
			}
		}
	}
}
