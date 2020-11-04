using Dapper;
using Dapper.Contrib.Extensions;
using DHCPServer.Dapper.Context;
using DHCPServer.Domain.Enumerations;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

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

		public async Task<IEnumerable<Report>> GetReportsByString(string searchingString, Specification specification)
		{
			string query = @"SELECT *FROM Reports as r
						   Left join ActiveDevices as ad on r.id=ad.reportid
						   Left join Devices as d on d.id=ad.deviceid
						   Where r.Title like @title";
			var qwe = query.Replace("@title", searchingString);
			var lookup = new Dictionary<int, Report>();

			using (var connection = _factory.CreateConnection())
			{
				switch (specification)
				{
					case Specification.IpAddress:
						throw new NotImplementedException();
						break;
					case Specification.Report:
						var entity = await connection.QueryAsync<Report,ActiveDevice,Device,Report>(query,
							(report, activeDevice, device) =>
							{

								if (!lookup.TryGetValue(report.Id, out Report r))
								{
									lookup.Add(report.Id, r = report);
								}
								activeDevice.Device = device;

								r.ActiveDevices.Add(activeDevice);
								return r;

							}, new { title = "%"+ searchingString+"%" });
						break;
					default:
						break;
				}
			}
			return lookup.Values;
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
