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
	public class ActiveDeviceRepository : IActiveDeviceRepository
	{
		private readonly ApplicationContextFactory _factory;

		public ActiveDeviceRepository(ApplicationContextFactory factory)
		{
			_factory = factory;
		}
		public async Task<IEnumerable<ActiveDevice>> GetActiveDevicesLists()
		{
			string query = @"SELECT *FROM ActiveDevices as ad 
							left join Devices as d on d.id=ad.deviceId
							where ad.isAdded = 1 and ad.isActive=1";
			using (var connection = _factory.CreateConnection())
			{
				var entities = await connection.QueryAsync<ActiveDevice, Device, ActiveDevice>(query,
					(a, d) =>
					{
						a.Device = d;
						return a;
					});

				return entities;
			}
		}

		public async Task<IEnumerable<ActiveDevice>> GetAppropriateDevicesLists()
		{
			string query = @"SELECT *FROM ActiveDevices as ad 
							left join Devices as d on d.id=ad.deviceId
							where ad.isActive=1";
			using (var connection = _factory.CreateConnection())
			{
				var entities = await connection.QueryAsync<ActiveDevice, Device, ActiveDevice>(query,
					(a, d) =>
					{
						a.Device = d;
						return a;
					});

				return entities;
			}
		}



		public async Task<ActiveDevice> CheckDevice(ActiveDevice activeDevice)
		{
			string query = "SELECT *From ActiveDevices Where id=@id";
			using (var connection = _factory.CreateConnection())
			{
				var device = await connection.QueryFirstOrDefaultAsync<ActiveDevice>(query, new { id = activeDevice.Id });
				if (device != null)
				{
					device.IsActive = true;
					await connection.UpdateAsync(device);
				}
				else
				{
					activeDevice.Id = await connection.InsertAsync(device);
				}
				return activeDevice;

			}
		}

		public async Task<bool> DeatachDevice(ActiveDevice activeDevice)
		{
			using (var connection = _factory.CreateConnection())
			{
				activeDevice.IsAdded = false;
				var b = await connection.UpdateAsync(activeDevice);
				return b;
			}
		}

		public async Task<int> DeatachDevices(IEnumerable<ActiveDevice> activeDevices)
		{
			using (var connection = _factory.CreateConnection())
			{
				int c = 0;
				foreach (var device in activeDevices)
				{
					device.IsAdded = false;
					var b = await connection.UpdateAsync(device);
					if (b)
						c++;
				}

				return c;
			}
		}

		public async Task<IEnumerable<ActiveDevice>> CheckDevices(IEnumerable<ActiveDevice> activeDevices)
		{
			string query = "SELECT *From ActiveDevices Where id=@id";
			using (var connection = _factory.CreateConnection())
			{
				foreach (var device in activeDevices)
				{
					var d = await connection.QueryFirstOrDefaultAsync<ActiveDevice>(query, new { id = device.Id });

					if (d != null)
					{
						d.IsActive = true;
						d.IsAdded = true;
						await connection.UpdateAsync(d);
						device.Set(d);
					}
					else
					{
						device.IsActive = true;
						device.IsAdded = true;
						device.Id = await connection.InsertAsync(device);
					}

				}

				return activeDevices;

			}
		}
	}
}
