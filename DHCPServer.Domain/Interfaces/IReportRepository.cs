using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Domain.Interfaces
{
	public interface IReportRepository
	{
		Task<Report> CreateAsync(Report report);
		Task<bool> UpdateAsync(Report report);

		Task<Report> GetLastReportByDeviceId(int deviceId);
		Task<Report> GetLastReport();
	}
}
