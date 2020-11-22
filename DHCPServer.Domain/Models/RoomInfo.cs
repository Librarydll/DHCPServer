using Dapper.Contrib.Extensions;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models.Common;
using Newtonsoft.Json;
using System;

namespace DHCPServer.Domain.Models
{
    public class RoomInfo : BaseEntity, IRoom
    {
        private double temperature;
        private double humidity;
        private ActiveDevice device;
        [JsonProperty("tt1")]
        public double Temperature
        {
            get => temperature;
            set => SetProperty(ref temperature, value);

        }
        [JsonProperty("hh1")]
        public double Humidity
        {
            get => humidity;
            set => SetProperty(ref humidity, value);
        }

        public DateTime Date { get; set; }


        public int DeviceId { get; set; }
        [Computed]

        public ActiveDevice ActiveDevice { get => device; set { device = value; RaisePropertyChangedEvent(); } }

        public RoomInfo(ActiveDevice device)
        {
            ActiveDevice = device;
            Date = DateTime.Now;
            DeviceId = device.Id;
        }
        public RoomInfo()
        { }



    }
}
