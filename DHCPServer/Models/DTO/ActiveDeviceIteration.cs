using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Models.DTO
{
    public class ActiveDeviceIteration : Device
    {
        public int Number { get; set; }
    }

    public static class ActiveDeviceIterationExtenstion
    {
        public static IEnumerable<ActiveDeviceIteration> Map(this IEnumerable<ActiveDevice> activeDevices)
        {
            return activeDevices.Select((x, i) => new ActiveDeviceIteration
            {
                Id = x.Id,
                IPAddress = x.IPAddress,
                Nick = x.Nick,
                Number = i + 1
            });
        }
    }
}
