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

		Task<bool> UpdateDevices(Device newDivece, Device oldDevice);


	}
}
