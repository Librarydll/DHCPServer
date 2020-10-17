using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.Context
{
	public class ApplicationContextFactory
	{
		private static readonly string connString = "Data Source=data2.db;";
		public IDbConnection CreateConnection()
		{
			return new SQLiteConnection(connString);
		}
	}
}
