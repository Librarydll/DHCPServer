﻿using DHCPServer.Domain.Enumerations;
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

		Task<Report> GetLastReport();
		/// <summary>
		/// 
		/// </summary>
		/// <param name="searchingString"></param>
		/// <returns></returns>
		Task<IEnumerable<Report>> GetReportsByString(string searchingString,Specification specification);

		Task<bool> TryCloseReport(ActiveDevice device);
		Task<IEnumerable<Report>> TryCloseExpiredReports();

		Task<Report> CreateReport(Report report,IEnumerable<ActiveDevice> activeDevices);

		Task<IEnumerable<Report>> GetActiveReports();
		Task<IEnumerable<Report>> GetActiveReportsWithDevices();
	}
}
