﻿using Dapper;
using Dapper.Contrib.Extensions;
using DHCPServer.Dapper.Context;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DHCPServer.Dapper.Repositories
{
	public class RoomRepository : IRoomRepository
	{
		private readonly ApplicationContextFactory _factory;
		public RoomRepository()
		{
			_factory = new ApplicationContextFactory();
		}


		public async Task<RoomInfo> SaveAsync(RoomInfo roomInfo)
		{
			using (var connection = _factory.CreateConnection())
			{
				roomInfo.Id = await connection.InsertAsync(roomInfo);
				return roomInfo;
			}
		}
		public async Task SaveAsync(IEnumerable<RoomInfo> roomInfos)
		{
			using (var connection = _factory.CreateConnection())
			{
				foreach (var room in roomInfos)
				{
					room.Id = await connection.InsertAsync(room);
				}
			}
		}

		public async Task<IEnumerable<RoomInfo>> FilterRooms(DateTime from, DateTime to)
		{
			string query = @"SELECT *FROM RoomInfos as r 
							Left join ActiveDevices as d on 
							r.deviceid=d.id where date>=@from and date<=@to";
			using (var connection = _factory.CreateConnection())
			{
				var result = await connection.QueryAsync<RoomInfo, ActiveDevice, RoomInfo>(query,
					(r, d)
					=>
					{
						r.ActiveDevice = d;
						return r;
					},
					new
					{ from = from.Date.ToString("yyyy-MM-dd"), to = to.Date.ToString("yyyy-MM-dd") });
				return result;
			}
		}

		public async Task<IEnumerable<RoomInfo>> FilterRooms(DateTime fromDate, DateTime toDate, TimeSpan fromTime, TimeSpan toTime)
		{
			string query = @"SELECT *FROM RoomInfos as r 
						   Left join ActiveDevices as d on r.deviceid=d.id 
						   where date>=@from and date<=@to";

			using (var connection = _factory.CreateConnection())
			{
				var fromD = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, fromTime.Hours, fromTime.Minutes, 0);
				var toD = new DateTime(toDate.Year, toDate.Month, toDate.Day, toTime.Hours, toTime.Minutes, 0);
				var result = await connection.QueryAsync<RoomInfo,ActiveDevice, RoomInfo>(query, (r, d)
					 =>
				{
					r.ActiveDevice = d;

					return r;
				},
				new
				{ from = fromD.ToString("yyyy-MM-dd HH:mm:ss"), to = toD.ToString("yyyy-MM-dd HH:mm:ss") });
				return result;
			}
		}

		public async Task<IEnumerable<RoomInfo>> FilterRooms(int deviceid, DateTime date,TimeSpan fromTime, TimeSpan toTime)
		{
			string query = "SELECT *FROM RoomInfos where deviceid =@deviceid and ( date>=@from and date<=@to)";

			using (var connection = _factory.CreateConnection())
			{
				var fromD = new DateTime(date.Year, date.Month, date.Day, fromTime.Hours, fromTime.Minutes, 0);
				var toD = new DateTime(date.Year, date.Month, date.Day, toTime.Hours, toTime.Minutes, 0);
				var result = await connection.QueryAsync<RoomInfo>(query, new
				{ from = fromD.ToString("yyyy-MM-dd HH:mm:ss"), to = toD.ToString("yyyy-MM-dd HH:mm:ss"), deviceid });
				return result;
			}
		}

		public async Task<IEnumerable<RoomInfo>> FilterRooms(int deviceid, DateTime date)
		{
			string query = "SELECT *FROM RoomInfos as r left join ActiveDevices as d on r.deviceid=@id where  date(date)=@date";

			using (var connection = _factory.CreateConnection())
			{

				var result = await connection.QueryAsync<RoomInfo, ActiveDevice, RoomInfo>(query, (r, d)
					 =>
				{
					r.ActiveDevice = d;
					return r;
				},
				new
				{ id = deviceid, date = date.Date.ToString("yyyy-MM-dd") });
				return result;
			}
		}

		public async Task<IEnumerable<RoomInfo>> FilterRooms(int reportId,int deviceId)
		{
			string query = @"SELECT *FROM RoomInfos as r 
						   left join activedevices as ad on r.deviceid=ad.id
						   left join reports as rep on rep.id = ad.reportid
						   Where ad.reportid=@reportId and r.deviceId=@deviceId and 
						   (r.date >=rep.FromTime and r.date<=datetime(rep.FromTime ,'+'||rep.Days||' days'))
						   order by r.Date";

			using (var connection = _factory.CreateConnection())
			{
				var result = await connection.QueryAsync<RoomInfo, ActiveDevice,Report, RoomInfo>(query, (r,ad, repo)
					 =>
				{			
					r.ActiveDevice = ad;
					if (r.ActiveDevice == null)
						r.ActiveDevice = new ActiveDevice();

					return r;
				},
				new
				{ reportId, deviceId });
				return result;
			}
		}

		public async Task<IEnumerable<RoomInfo>> FilterRooms(string ipAddress, DateTime from, DateTime to)
		{
			string query = @"SELECT *FROM RoomInfos as r 
							Left join ActiveDevices as d on 
							r.deviceid=d.id where date>=@from and date<=@to and d.ipaddress=@ipAddress";
			using (var connection = _factory.CreateConnection())
			{
				var result = await connection.QueryAsync<RoomInfo, ActiveDevice, RoomInfo>(query,
					(r, d)
					=>
					{
						r.ActiveDevice = d;
						return r;
					},
					new
					{ from = from.Date.ToString("yyyy-MM-dd"), to = to.Date.ToString("yyyy-MM-dd") ,ipAddress });
				return result;
			}
		}
		public async Task<IEnumerable<RoomInfo>> FilterRooms(int deviceId)
		{
			string query = @"SELECT *FROM RoomInfos as r 
							 where r.DeviceId =@deviceId";

			using (var connection = _factory.CreateConnection())
			{
				var result = await connection.QueryAsync<RoomInfo>(query,new { deviceId });
				return result;
			}
		}
	}
}
