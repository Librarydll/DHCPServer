using Dapper.Contrib.Extensions;
using DHCPServer.Dapper.Context;
using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DHCPServer.Models
{
	public class Transfer
	{
		private readonly ApplicationContextFactory _factory;

		public Transfer(ApplicationContextFactory factory)
		{
			_factory = factory;
		}
		public async Task TransferData()
		{
			List<RoomInfo> roomInfos = new List<RoomInfo>();
			using (var file =new StreamReader("log.log"))
			{
				int i = 0;
				string line;
				RoomInfo room=null;
				while (( line = await file.ReadLineAsync())!=null)
				{
					if(!line.Contains("ADDED To Graph"))
					{
						room = new RoomInfo();
						   i = 0;
						continue;
					}
					i++;
					var time = line.Split().TakeWhile(x => x != "[INF]");
					var agg = time.Aggregate((a, b) => a + " " + b);
					var t = DateTime.Parse(agg.Trim());
					room.Date = t;
					room.DeviceId = 2;
					if (i == 1)
					{
						var h = line.Split().ElementAt(11);
						room.Humidity = double.Parse(h.Replace(".",","));
					}
					if (i == 2)
					{
						var te = line.Split().ElementAt(11);
						room.Temperature = double.Parse(te.Replace(".", ","));
						roomInfos.Add(room);
					}
				}
			}

			using (var connection = _factory.CreateConnection())
			{
				foreach (var item in roomInfos)
				{
					await connection.InsertAsync(item);
				}
			}
		}
	}
}
