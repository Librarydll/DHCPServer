using DHCPServer.Domain.Enumerations;
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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="searchingString"></param>
		/// <returns></returns>
		Task<Report> GetReport(string searchingString,Specification specification);
	}
}
