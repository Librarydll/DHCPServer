using Dapper.Contrib.Extensions;
using DHCPServer.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Domain.Models
{
	public class ActiveDevice : Device
	{
		public ActiveDevice()
		{
			RoomInfos = new List<RoomInfo>();
		}
        public ActiveDevice(Device device):this()
        {
			IPAddress = device?.IPAddress;
			Nick = device?.Nick;
        }
		private bool _isActive;
		public bool IsActive
		{
			get { return _isActive; }
			set { SetProperty(ref _isActive, value); }
		}
		private bool isAdded;

		public bool IsAdded
		{
			get { return isAdded; }
			set { SetProperty(ref isAdded, value); }
		}

		public int ReportId { get; set; }
		[Computed]
		public Report Report { get; set; }

		[Computed]
		public ICollection<RoomInfo> RoomInfos { get; set; }

		public void Set(ActiveDevice device)
		{
			Id = device.Id;
            IsActive = device.IsActive;
			IsAdded = device.IsAdded;
			ReportId = device.ReportId;
			IPAddress = device?.IPAddress;
			Nick = device?.Nick;
		}

		public void Set(Device device)
        {
			IPAddress = device?.IPAddress;
			Nick = device?.Nick;
        }
	}
}
