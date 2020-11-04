using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.DTO
{
	public class ReportDTO
	{
		public Report Report { get; set; }
		public ActiveDevice ActiveDevice { get; set; }


		public static IEnumerable<ReportDTO> Map(IEnumerable<Report> reports)
		{
			ICollection<ReportDTO> result = new List<ReportDTO>();

			foreach (var report in reports)
			{
				foreach (var device in report.ActiveDevices)
				{
					ReportDTO r = new ReportDTO
					{
						ActiveDevice = device,
						Report = report
					};
					result.Add(r);
				}
			}
			return result;
		}
	}
}
