using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Services
{
	public interface IParser<out TRoom> where TRoom : RoomInfo
	{
		TRoom Parse(string content);
	}
}
