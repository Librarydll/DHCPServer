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
			+ "\"IsClosed\"	INTEGER NOT NULL DEFAULT 0,"
			+ "\"Days\"	INTEGER NOT NULL DEFAULT 0);";
		public static string dropDeviceListTable = "DROP TABLE IF EXISTS DevicesLists;";

		public static string alterDeviceQuery = "BEGIN TRANSACTION;"
							+ "CREATE TEMPORARY TABLE t1_backup(Id, IpAddress, Nick);"
							+ "INSERT INTO t1_backup SELECT Id,IpAddress,Nick FROM  Devices;"
							+ "DROP TABLE Devices;"
							+ "CREATE TABLE `Devices` ("
							+ "`Id`	INTEGER PRIMARY KEY AUTOINCREMENT,"
							+ "`IPAddress`	TEXT NOT NULL UNIQUE,"
							+ "`Nick`	TEXT UNIQUE"
							+ "); INSERT INTO Devices SELECT Id,IpAddress,Nick FROM t1_backup;"
							+ "DROP TABLE t1_backup;"
							+ "COMMIT;";


		public static string createActiveDeviceTable = "CREATE TABLE IF NOT EXISTS `ActiveDevices` ("
							+ "`Id`	INTEGER PRIMARY KEY AUTOINCREMENT,"
							+ "	`IPAddress`	TEXT NOT NULL,"
							+ "`Nick`		TEXT NOT NULL,"
							+ "`IsActive`	INTEGER NOT NULL DEFAULT 0,"
							+ "`IsAdded`	INTEGER NOT NULL DEFAULT 0,"
							+ "`ReportId`	INTEGER NOT NULL,"
							+ "FOREIGN KEY(`ReportId`) REFERENCES `Reports`(`id`));";

		public static string alterRoomInfos = "BEGIN TRANSACTION;"
							+ "CREATE TEMPORARY TABLE t1_backup(Id, Temperature, Humidity, Date, DeviceId);"
							+ "INSERT INTO t1_backup SELECT Id,Temperature,Humidity,Date,DeviceId FROM RoomInfos;"
							+ "DROP TABLE RoomInfos;"
							+ "CREATE TABLE `RoomInfos` ("
							+ "`Id`	INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE,"
							+ "`Temperature`	REAL,"
							+ "`Humidity`	REAL,"
							+ "`Date`	TEXT,"
							+ "`DeviceId`	INTEGER,"
							+ "FOREIGN KEY(`DeviceId`) REFERENCES `ActiveDevices`(`id`));"
							+ "INSERT INTO RoomInfos SELECT Id,Temperature,Humidity,Date,DeviceId FROM t1_backup;"
							+ "DROP TABLE t1_backup; COMMIT;";

		public static string dropReportTable = "Drop Table IF EXISTS Reports";



	}
}
