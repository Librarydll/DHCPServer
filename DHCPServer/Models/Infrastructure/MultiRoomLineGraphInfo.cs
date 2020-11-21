using DHCPServer.Domain.Models;
using DHCPServer.Models.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.Infrastructure
{
	public class MultiRoomLineGraphInfo : RoomLineBase<ActiveDevice, MultiRoomInfo>
	{
		public MultiRoomLineGraphInfo(ActiveDevice device) : base(device)
		{
		}


		public override void SetInvalid(bool value)
		{
			
		}
	}
}
