using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Console.Script
{
	public static class Query
	{


		public static string updateDeviceQuery = "ALTER TABLE Devices add column IsAdded INTEGER";

		public static string createReportTable = $"CREATE TABLE \"Reports\" (" 
			+"\"Id\"	INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE," 
			+"\"Title\"	TEXT NOT NULL," 
			+"\"LastUpdated\"	TEXT," 
			+"\"FromTime\"	TEXT," 
			+"\"ToTime\"	TEXT," 
			+"\"DeviceId\"	INTEGER,"
			+"FOREIGN KEY(\"DeviceId\") REFERENCES \"Devices\"(\"id\"));";
		public static string dropDeviceListTable = "DROP TABLE IF EXISTS DevicesLists;";
	}
}
