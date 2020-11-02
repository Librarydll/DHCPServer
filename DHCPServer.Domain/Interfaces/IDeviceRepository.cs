using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Domain.Interfaces
{
	public interface IDeviceRepository
	{
		Task<Device> CreateAsync(Device device);
		Task<bool> DeleteAsync(Device device);

		Task<bool> UpdateAsync(Device device);
		Task<int> UpdateRangeAsync(IEnumerable<Device> devices);
		Task<IEnumerable<Device>> GetAllAsync();

		Task<IEnumerable<ActiveDevice>> GetActiveDevicesLists();

		/// <summary>
		/// Check if activedevice is already exist,it will set active as true
		/// otherwise it will create new one
		/// </summary>
		/// <returns></returns>
		Task<ActiveDevice> CheckDevice(ActiveDevice activeDevice);

		/// <summary>
		/// Check if activedevice is already exist,it will set active as true
		/// otherwise it will create new one
		/// </summary>
		/// <returns></returns>
		Task<IEnumerable<ActiveDevice>> CheckDevices(IEnumerable<ActiveDevice> activeDevice);

		/// <summary>
		/// Set device as inactive
		/// </summary>
		/// <returns></returns>
		Task<bool> InactiveDevice(ActiveDevice activeDevice);
		Task<int> InactiveDevices(IEnumerable<ActiveDevice> activeDevices);


	}
}
