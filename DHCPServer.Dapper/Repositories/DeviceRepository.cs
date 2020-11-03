using Dapper;
using Dapper.Contrib.Extensions;
using DHCPServer.Dapper.Context;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DHCPServer.Dapper.Repositories
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
			string query = @"SELECT *FROM DEVICES as d
							Left Join ActiveDevices as ad
							on d.id = ad.deviceid
							where ad.isActive=1 or ad.isActive is null";
			using (var connection = _factory.CreateConnection())
			{
				var entities = await connection.QueryAsync<Device,ActiveDevice,Device>(query,
					(d,ad)=> 
					{
						d.ActiveDevice = ad;
						if (d.ActiveDevice == null)
						{
							d.ActiveDevice = new ActiveDevice();
						}
						return d;
					});
				return entities;
			}
		}


		public async Task<IEnumerable<Device>> GetDevicesLists()
		{
			string query = @"SELECT *FROM Devices where isAdded = 1";
			using (var connection = _factory.CreateConnection())
			{
				var entities = await connection.QueryAsync<Device>(query);

				return entities;
			}
		}

		public async Task<int> UpdateRangeAsync(IEnumerable<Device> devices)
		{
			using (var connection = _factory.CreateConnection())
			{
				int n = 0;
				foreach (var device in devices)
				{
					if (await connection.UpdateAsync(device)) n++;
				}
				
				return n;
			}
		}


	}
}
