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
			using (var connection = _factory.CreateConnection())
			{
				var entities = await connection.GetAllAsync<Device>();
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

        public async Task<bool> UpdateDevices(Device newDivece, Device oldDevice)
        {
			using (var connection = _factory.CreateConnection())
			{
				var query = "Select *from activedevices where ipaddress=@address and isactive=1 and isadded=1";
				var isUpdated = await connection.UpdateAsync(newDivece);
				var device = await connection.QueryFirstOrDefaultAsync<ActiveDevice>(query,new { address=oldDevice.IPAddress});
                if (device !=null)
                {
					device.Set(newDivece);
				}
				await connection.UpdateAsync(device);
				return isUpdated;
			}
		}
    }
}
