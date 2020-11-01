using Dapper;
using Dapper.Contrib.Extensions;
using DHCPServer.Models.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.Repositories
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

		public async Task<Report> GetLastReport(int deviceId)
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
