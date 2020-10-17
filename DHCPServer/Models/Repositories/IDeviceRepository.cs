using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.Repositories
{
	public interface IDeviceRepository
	{
		Task<Device> CreateAsync(Device device);
		Task<bool> DeleteAsync(Device device);

		Task<bool> UpdateAsync(Device device);
		Task<IEnumerable<Device>> GetAllAsync();

		Task<IEnumerable<Device>> GetDevicesLists();

		Task<DevicesList> CreateDeviceListAsync(DevicesList device);
		Task<bool> DeleteDeviceListAsync(int deviceId);
	}
}
