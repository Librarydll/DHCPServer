using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.Repositories
{
	public interface IReportRepository
	{
		Task<Report> CreateAsync(Report report);
		Task<bool> UpdateAsync(Report report);

		Task<Report> GetLastReport(int deviceId);
	}
}
