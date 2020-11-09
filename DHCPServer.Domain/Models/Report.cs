using Dapper.Contrib.Extensions;
using DHCPServer.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Domain.Models
{
	public class Report : BaseEntity
	{
		public Report()
		{
			ActiveDevices = new List<ActiveDevice>();
		}
		public Report(Report report) : this()
		{
			Days = report.Days;
			Id = report.Id;
			FromTime = report.FromTime;
			LastUpdated = report.LastUpdated;
			IsClosed = report.IsClosed;
			Title = report.Title;
		}
		private string _title;
		public string Title
		{
			get { return _title; }
			set { SetProperty(ref _title, value); }
		}
		public DateTime LastUpdated { get; set; }

		private DateTime _fromTime;
		public DateTime FromTime
		{
			get { return _fromTime; }
			set { SetProperty(ref _fromTime, value); }
		}
		private int _days;
		public int Days
		{
			get { return _days; }
			set { SetProperty(ref _days, value); RaisePropertyChangedEvent("ToTime"); }
		}

		private bool _isClosed;
		public bool IsClosed
		{
			get { return _isClosed; }
			set { SetProperty(ref _isClosed, value); }
		}
		[Computed]
		public ICollection<ActiveDevice> ActiveDevices { get; set; }

		[Computed]
		public DateTime ToTime => FromTime.AddDays(Days);
		public bool IsEdited(Report newReport)
		{
			if (newReport == null) throw new ArgumentNullException("newReport is null");
			if (Title != newReport.Title)
			{
				return true;
			}
			if (Days != newReport.Days)
			{
				return true;
			}

			if (FromTime != newReport.FromTime)
			{
				return true;
			}
			return false;
		}

	}
}
