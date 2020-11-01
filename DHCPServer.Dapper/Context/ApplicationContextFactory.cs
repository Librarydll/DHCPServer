using System.Data;
using System.Data.SQLite;

namespace DHCPServer.Dapper.Context
{
	public class ApplicationContextFactory
	{
		private static readonly string connString = "Data Source=data.db;";
		public IDbConnection CreateConnection()
		{
			return new SQLiteConnection(connString);
		}
	}
}
