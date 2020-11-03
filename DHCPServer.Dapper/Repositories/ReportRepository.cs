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
	public class ReportRepository : IReportRepository
	{
		private readonly ApplicationContextFactory _factory;

		public ReportRepository()
		{
			_factory = new ApplicationContextFactory();
		}

		public async Task<Report> CreateAsync(Report report)
		{
			using (var connection = _factory.CreateConnection())
			{
				int id = await connection.InsertAsync(report);
				report.Id = id;
				return report;
			}
		}

		public async Task<Report> GetLastReport()
		{
			string query = "SELECT *FROM Reports order by id desc  LIMIT 1";
			using (var connection = _factory.CreateConnection())
			{
				var report = await connection.QueryFirstOrDefaultAsync<Report>(query);
				return report;
			}
		}

		public async Task<Report> GetLastReportByDeviceId(int deviceId)
		{
			string query = "SELECT *FROM Reports Where deviceid=@deviceId order by id desc  LIMIT 1";
			using (var connection = _factory.CreateConnection())
			{
				var report = await connection.QueryFirstOrDefaultAsync<Report>(query, new { deviceId });
				return report;
			}
		}

		public async Task<bool> UpdateAsync(Report report)
		{
			using (var connection = _factory.CreateConnection())
			{
				var isUpdated = await connection.UpdateAsync(report);
				return isUpdated;
			}
		}
	}
}
