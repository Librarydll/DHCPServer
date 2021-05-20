using Dapper;
using Dapper.Contrib.Extensions;
using DHCPServer.Dapper.Context;
using DHCPServer.Domain.Enumerations;
using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Dapper.Repositories
{
    public class ActiveDeviceRepository : IActiveDeviceRepository
    {
        private readonly ApplicationContextFactory _factory;

        public ActiveDeviceRepository(ApplicationContextFactory factory)
        {
            _factory = factory;
        }
        public async Task<IEnumerable<ActiveDevice>> GetActiveDevicesListsWithReports()
        {
            string query = @"SELECT *FROM ActiveDevices as ad
                            Left join Reports as r on ad.reportid=r.id
							where ad.isAdded = 1 and ad.isActive=1 and r.isclosed=0";
            using (var connection = _factory.CreateConnection())
            {
                var entities = await connection.QueryAsync<ActiveDevice, Report, ActiveDevice>(query,
                    (ad, r) =>
                    {
                        ad.Report = r;
                        ad.ReportId = r.Id;
                        return ad;
                    });

                return entities;
            }
        }


        public async Task<ActiveDevice> CheckDevice(ActiveDevice activeDevice)
        {
            string query = "SELECT *From ActiveDevices Where id=@id";
            using (var connection = _factory.CreateConnection())
            {
                var device = await connection.QueryFirstOrDefaultAsync<ActiveDevice>(query, new { id = activeDevice.Id });
                if (device != null)
                {
                    device.IsActive = true;
                    await connection.UpdateAsync(device);
                }
                else
                {
                    activeDevice.Id = await connection.InsertAsync(activeDevice);
                }
                return activeDevice;

            }
        }

        public async Task<bool> DeatachDevice(ActiveDevice activeDevice)
        {
            using (var connection = _factory.CreateConnection())
            {
                activeDevice.IsAdded = false;
                var b = await connection.UpdateAsync(activeDevice);
                return b;
            }
        }

        public async Task<int> DeatachDevices(IEnumerable<ActiveDevice> activeDevices)
        {
            using (var connection = _factory.CreateConnection())
            {
                int c = 0;
                foreach (var device in activeDevices)
                {
                    device.IsAdded = false;
                    var b = await connection.UpdateAsync(device);
                    if (b)
                        c++;
                }

                return c;
            }
        }

        public async Task<IEnumerable<ActiveDevice>> CheckDevices(IEnumerable<ActiveDevice> activeDevices)
        {
            string query = "SELECT *From ActiveDevices Where id=@id";
            using (var connection = _factory.CreateConnection())
            {
                foreach (var device in activeDevices)
                {
                    var d = await connection.QueryFirstOrDefaultAsync<ActiveDevice>(query, new { id = device.Id });

                    if (d != null)
                    {
                        d.IsActive = true;
                        d.IsAdded = true;
                        await connection.UpdateAsync(d);
                        device.Set(d);
                    }
                    else
                    {
                        device.IsActive = true;
                        device.IsAdded = true;
                        device.Id = await connection.InsertAsync(device);
                    }

                }

                return activeDevices;

            }
        }

        public async Task<IEnumerable<ActiveDevice>> GetActiveDevicesByReportId(int reportId)
        {
            string query = @"SELECT *FROM ActiveDevices 
							where reportid=@reportId and isactive=1";
            using (var connection = _factory.CreateConnection())
            {
                var entities = await connection.QueryAsync<ActiveDevice>(query, new { reportId });

                return entities;
            }
        }

        public async Task<bool> UpdateAsync(ActiveDevice activeDevice)
        {
            using (var connection = _factory.CreateConnection())
            {
                var b = await connection.UpdateAsync(activeDevice);
                return b;
            }
        }

        public async Task<IEnumerable<ActiveDevice>> GetActiveDevicesLists(DeviceType deviceType = DeviceType.Default)
        {
            string query = @"SELECT *FROM ActiveDevices as ad
                            left join Reports as r
                            on r.id = ad.ReportId
                            left join DeviceSettings as ds
                            on ds.ActiveDeviceId = ad.Id
							where ad.isAdded = 1 and ad.isActive=1 and DeviceType =@deviceType";
            using (var connection = _factory.CreateConnection())
            {
                var entities = await connection.QueryAsync<ActiveDevice,Report,DeviceSetting,ActiveDevice>(query,
                    (activeDevice,report, deviceSetting) =>
                    {
                        activeDevice.Report = report;
                        activeDevice.DeviceSetting = deviceSetting ?? new DeviceSetting();
                        return activeDevice;
                    },
                    new { deviceType });

                return entities;
            }
        }

        public async Task<IEnumerable<ActiveDevice>> GetActiveDevicesByDate(DateTime from, DateTime to)
        {
            string query = @"SELECT *FROM ActiveDevices as ad 
							Left join RoomInfos as r  on
                            r.deviceid=ad.id 
                            Left join MultiRoomInfos as mr on
							mr.DeviceId =ad.id
							where( r.date>=@from and r.date<=@to )
							or( mr.date>=@from and mr.date<=@to )";
            var lookup = new Dictionary<int, ActiveDevice>();

            using (var connection = _factory.CreateConnection())
            {
                var result = await connection.QueryAsync<ActiveDevice, RoomInfo, ActiveDevice>(query,
                    (activeDevice, r)
                    =>
                    {
                        if (!lookup.TryGetValue(activeDevice.Id, out ActiveDevice ad))
                        {
                            lookup.Add(activeDevice.Id, ad = activeDevice);
                        }
                        activeDevice.RoomInfos.Add(r);
                        return activeDevice;
                    },
                    new
                    { from = from.Date.ToString("yyyy-MM-dd"), to = to.Date.ToString("yyyy-MM-dd") });
                return lookup.Values;
            }
        }

        public async Task<IEnumerable<ActiveDevice>> GetActiveDeviceWithoutReports()
        {

            string query = @"SELECT *FROM ActiveDevices as ad
							where ad.isAdded = 1 and ad.isActive=1 and ad.DeviceType =0 and ad.ReportId=0";
            using (var connection = _factory.CreateConnection())
            {
                var entities = await connection.QueryAsync<ActiveDevice>(query);

                return entities;
            }
        }

        public async Task SwapReportId(ActiveDevice from, ActiveDevice to)
        {

            using (var connection = _factory.CreateConnection())
            {
                string query = @"UPDATE ActiveDevices SET reportid = 0  Where id=@a1;
                                 UPDATE ActiveDevices SET reportid = @r1  Where id=@a2;";

                var entities = await connection.ExecuteAsync(query, new { a1 = from.Id, a2 = to.Id, r1 = from.ReportId });

            }
        }
    }
}
