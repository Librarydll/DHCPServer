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
					connection.Execute(Query.alterDeviceQuery);				
					connection.Execute(Query.createActiveDeviceTable);					
					connection.Execute(Query.alterRoomInfos);
					connection.Execute(Query.dropReportTable);
					connection.Execute(Query.createReportTable);

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
