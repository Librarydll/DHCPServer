using Dapper;
using Dapper.Contrib.Extensions;
using DHCPServer.Dapper.Context;
using DHCPServer.Domain.Enumerations;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

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

		public async Task<Report> CreateReport(Report report, IEnumerable<ActiveDevice> activeDevices)
		{
			using (var connection = _factory.CreateConnection())
			{
				int id = await connection.InsertAsync(report);
				report.Id = id;
				foreach (var device in activeDevices)
				{
					device.ReportId = id;
					device.Id = await connection.InsertAsync(device);
				}
				return report;
			}
		}

		public async Task<IEnumerable<Report>> GetActiveReports()
		{
			string query = "SELECT *FROM Reports WHERE IsClosed = 0";
			using (var connection = _factory.CreateConnection())
			{
				var report = await connection.QueryAsync<Report>(query);
				return report;
			}
		}

		public async Task<IEnumerable<Report>> GetActiveReportsWithDevices()
		{
			string query = @"SELECT *FROM Reports as r
							Left join ActiveDevices as ad on ad.reportid = r.id
							WHERE r.IsClosed = 0";
			var lookup = new Dictionary<int, Report>();

			using (var connection = _factory.CreateConnection())
			{
				var reports = await connection.QueryAsync<Report, ActiveDevice, Report>(query,
					(report, activeDevice) =>
					{

						if (!lookup.TryGetValue(report.Id, out Report r))
						{
							lookup.Add(report.Id, r = report);
						}

						r.ActiveDevices.Add(activeDevice);
						return r;

					});
				return lookup.Values;
			}
		}

		public async Task<Report> GetLastReport()
		{
			string query = "SELECT *FROM Reports WHERE IsClosed = 0 order by id desc LIMIT 1";
			using (var connection = _factory.CreateConnection())
			{
				var report = await connection.QueryFirstOrDefaultAsync<Report>(query);
				return report;
			}
		}

		public async Task<IEnumerable<Report>> GetReportsByString(string searchingString, Specification specification)
		{
			string query = @"SELECT *FROM Reports as r
						   Left join ActiveDevices as ad on r.id=ad.reportid
						   Where r.Title like @title";
			var lookup = new Dictionary<int, Report>();

			using (var connection = _factory.CreateConnection())
			{
				switch (specification)
				{
					case Specification.IpAddress:
						throw new NotImplementedException();
						break;
					case Specification.Report:
						var entity = await connection.QueryAsync<Report, ActiveDevice, Report>(query,
							(report, activeDevice) =>
							{

								if (!lookup.TryGetValue(report.Id, out Report r))
								{
									lookup.Add(report.Id, r = report);
								}

								r.ActiveDevices.Add(activeDevice);
								return r;

							}, new { title = "%" + searchingString + "%" });
						break;
					default:
						break;
				}
			}
			return lookup.Values;
		}

        public async Task<IEnumerable<Report>> TryCloseExpiredReports()
        {

			string query = @"SELECT *FROM Reports as r 
							Left join ActiveDevices as ad on r.id = ad.reportid
							Where r.IsClosed=0 and (DATE() >=r.FromTime and DATE()<=datetime(r.FromTime ,'+'||r.Days||' days'))";
			var lookup = new Dictionary<int, Report>();

			using (var connection = _factory.CreateConnection())
			{

				var reports = await connection.QueryAsync<Report,ActiveDevice,Report>(query,
					(report, activeDevice) =>
					{

						if (!lookup.TryGetValue(report.Id, out Report r))
						{
							lookup.Add(report.Id, r = report);
						}

						r.ActiveDevices.Add(activeDevice);
						return r;

					});

                foreach (var r in reports)
                {
					r.IsClosed = true;
					await connection.UpdateAsync(r);
                    foreach (var d in r.ActiveDevices)
                    {
						d.IsActive = false;
						d.IsAdded = false;
						await connection.UpdateAsync(d);
                    }
                }

				return lookup.Values;
			}

		}

		public async Task<bool> TryCloseReport(ActiveDevice activeDevice)
		{
			if (activeDevice == null) return false;


			string updateQuery = @"UPDATE ActiveDevices SET IsActive=0,IsAdded=0 WHERE Id in
							(SELECT id FROM ActiveDevices where reportId = @reportid);";


			string query = "SELECT *FROM Reports Where id=@id;";

			using (var connection = _factory.CreateConnection())
			{

				var report = await connection.QueryFirstOrDefaultAsync<Report>(query, new { id = activeDevice.ReportId });
				if (report == null) return false;

				if (report.FromTime > DateTime.Now) return false;

				var rowCount = await connection.ExecuteAsync(updateQuery, new { reportid = report.Id });
				report.IsClosed = true;
				var x = await connection.UpdateAsync(report);
				return (rowCount > 0 && x);
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
