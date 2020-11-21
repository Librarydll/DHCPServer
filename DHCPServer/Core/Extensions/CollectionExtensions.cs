using DHCPServer.Domain.Models;
using DHCPServer.Models;
using DHCPServer.Models.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DHCPServer.Core.Extensions
{
	public static class CollectionextEnsioncs
	{

		
		public static void DisposeRange<T>(this IEnumerable<T> collection)where T:IDisposable
        {
            foreach (var item in collection)
            {
				item?.Dispose();
            }
        }
        public static IEnumerable<T> DisposeRange<T>(this IEnumerable<T> collection,Func<T,bool> action) where T : IDisposable
        {
            foreach (var item in collection)
            {
                if (action(item))
                {
                    item?.Dispose();
                    yield return item;
                }
            }
        }

        public static IEnumerable<ActiveDevice> CreateActiveDevices(this IEnumerable<Device> devices,IEnumerable<ActiveDevice> activeDevices)
        {

            foreach (var device in devices)
            {
				var ad = activeDevices.FirstOrDefault(x => x.IPAddress == device.IPAddress);
                if (ad != null)
                {
					yield return ad;
                }
                else
                {
					yield return new ActiveDevice(device);        
                }
            }
        }

		public static Report DistinctActiveDevice(this Report report
			,IEnumerable<Report> reports,IEnumerable<Device> devices)
        {
            foreach (var d in devices)
            {
                var ad = report.ActiveDevices.FirstOrDefault(x => x.IPAddress == d.IPAddress);
                if(ad==null)
                    report.ActiveDevices.Add(new ActiveDevice(d));
            }
            foreach (var r in reports)
            {
                if (r.Id == report.Id) continue;

                foreach (var activeDevice in r.ActiveDevices)
                {
                    var dev = r.ActiveDevices.FirstOrDefault(x => x.IPAddress == activeDevice.IPAddress);
                    if (dev != null)
                    {
                        if (dev.IsAdded)
                        {
                            var reps = report.ActiveDevices.FirstOrDefault(x => x.IPAddress == activeDevice.IPAddress);
                            report.ActiveDevices.Remove(reps);
                        }
                    }                  
                }
            }
       
            return report;
        }
	}
}
