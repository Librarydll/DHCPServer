using Dapper;
using DHCPServer.Dapper.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Console.Script
{
	public class ScriptManager
	{
		private ApplicationContextFactory _factory;
		public ScriptManager()
		{
			_factory = new ApplicationContextFactory();
		}


		public void Start()
		{

			using (var connection =_factory.CreateConnection())
			{
				try
				{
					var dropDevicesList = connection.Execute(Query.dropDeviceListTable);
					
					var alterTable = connection.Execute(Query.updateDeviceQuery);
					
					var create = connection.Execute(Query.createReportTable);
					System.Console.WriteLine("Success");
				}
				catch (Exception ex)
				{
					System.Console.WriteLine(ex.Message);
					System.Console.WriteLine(ex?.InnerException);
				}
			}

		}
	}
}
