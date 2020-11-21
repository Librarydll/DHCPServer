using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Services
{
	public class Parser<TRoom> : IParser<TRoom> 
		where TRoom :RoomInfo
	{
		public TRoom Parse(string content)
		{
			var type = typeof(TRoom);
			if (type==typeof(RoomInfo))
			{
				return (TRoom)content.ParseHtml();
			}
			else if(type ==typeof(MultiRoomInfo))
			{
				return content.ParseJson<TRoom>();
			}

			throw new ArgumentException("type is not valid while parsing content");
		}
	}
}
