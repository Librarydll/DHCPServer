using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Domain.Interfaces
{
	public interface IActiveDeviceRepository
	{
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
		/// Get List where isActive true
		/// </summary>
		/// <returns></returns>
		Task<IEnumerable<ActiveDevice>> GetAppropriateDevicesLists();

		/// <summary>
		/// Set device isadded false
		/// </summary>
		/// <returns>is operation successfully completed</returns>
		Task<bool> DeatachDevice(ActiveDevice activeDevice);
		/// <summary>
		/// Set devices isadded false
		/// </summary>
		/// <returns>row affected</returns>
		Task<int> DeatachDevices(IEnumerable<ActiveDevice> activeDevices);
	}
}
