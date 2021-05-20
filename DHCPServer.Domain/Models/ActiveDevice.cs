using Dapper.Contrib.Extensions;
using DHCPServer.Domain.Enumerations;
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
        public ActiveDevice(Device device) : base(device)
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
        private Report report;
        private bool isSelected;

        public bool IsAdded
        {
            get { return isAdded; }
            set { SetProperty(ref isAdded, value); }
        }
        public DeviceType DeviceType { get; set; }
        [Computed]

        public DeviceSetting DeviceSetting { get; set; }

        public int ReportId { get; set; }
        [Computed]
        public Report Report { get => report; set => SetProperty(ref report, value); }

        [Computed]
        public ICollection<RoomInfo> RoomInfos { get; set; }

        [Computed]
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
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

        public bool SameDevice(ActiveDevice activeDevice)
        {
            if (activeDevice == null) return false;

            if (activeDevice.IPAddress == IPAddress && activeDevice.Nick == Nick &&
               activeDevice.IsActive == IsActive && activeDevice.IsAdded == IsAdded)
                return true;
            return false;
        }

    }
}
