using Dapper.Contrib.Extensions;
using DHCPServer.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Domain.Models
{
	public class ActiveDevice : BaseEntity
	{
		public ActiveDevice()
		{
			RoomInfos = new List<RoomInfo>();
		}
		private bool _isActive;
		public bool IsActive
		{
			get { return _isActive; }
			set { SetProperty(ref _isActive, value); }
		}
		private bool isAdded;
		private Device device;

		public bool IsAdded
		{
			get { return isAdded; }
			set { SetProperty(ref isAdded, value); }
		}

		public int DeviceId { get; set; }
		public int ReportId { get; set; }
		[Computed]
		public Report Report { get; set; }

		[Computed]
		public Device Device { get => device; set => SetProperty(ref device, value); }

		[Computed]
		public ICollection<RoomInfo> RoomInfos { get; set; }

		public void Set(ActiveDevice device)
		{
			Id = device.Id;
            IsActive = device.IsActive;
			IsAdded = device.IsAdded;
			DeviceId = device.DeviceId;
			ReportId = device.ReportId;
		}
	}
}
