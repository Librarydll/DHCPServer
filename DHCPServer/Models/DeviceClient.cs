using DHCPServer.Models.Enums;
using DHCPServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DHCPServer.Models
{
	public class DeviceClient
	{
		public event Action<RoomInfo, DeviceResponseStatus> ReciveMessageEvent;
		public event Action<Device> ReciveMessageErrorEvent;
		public event Action<Device> EnableDeviceEvent;
		public Device Device { get; set; }
		private readonly HttpClient client = null;
		private readonly string _url = null;
		public bool IsInvalid => _countRequestForDisableInvaid!=0;
		private int _countRequestForDisableInvaid = 0;
		public DeviceClient(Device device)
		{
			if (device == null) throw new ArgumentNullException("device should not be null");
			if (device.IPAddress == null) throw new ArgumentNullException("device IPAddress should not be null");

			Device = device;
			client = new HttpClient
			{
				Timeout = new TimeSpan(0, 0, 5)
			};
			_url = "http://" + Device.IPAddress;
		}

		public async Task ListenAsync(CancellationToken token)
		{
			HttpResponseMessage response = null;

			while (true)
			{
				try
				{
					await Task.Delay(5000);

					if (!IsInvalid)
					{
						response = await client.GetAsync(_url, token);
						if (response.IsSuccessStatusCode)
						{
							string responseBody = await response.Content.ReadAsStringAsync();
							ReciveMessageRaise(responseBody, Device);
						}
						if (token.IsCancellationRequested)
						{
							token.ThrowIfCancellationRequested();
						}
					}
					else
					{
						_countRequestForDisableInvaid--;
						if (!IsInvalid)
							EnableDeviceEvent?.Invoke(Device);

					}

				}
				catch (InvalidOperationException e)
				{
				}
				catch (ArgumentException e)
				{

				}
				catch (HttpRequestException e)
				{
					_countRequestForDisableInvaid = 2;
					ReciveMessageErroRaise(Device);
				}
				catch (Exception ex)
				{
					_countRequestForDisableInvaid = 2;
					ReciveMessageErroRaise(Device);
				}
				finally
				{
					if (response != null) response.Dispose();
				} 
			}
		}

		private void ReciveMessageRaise(string responseBody, Device device, DeviceResponseStatus status = DeviceResponseStatus.Success)
		{
			if (status == DeviceResponseStatus.Success)
			{
				var room = HtmlHelper.Parse(responseBody);
				var result = new RoomInfo(room, device);
				ReciveMessageEvent?.Invoke(result, status);
			}
			else
			{
				ReciveMessageEvent?.Invoke(null, status);
			}
		}

		private void ReciveMessageErroRaise(Device invalidDevice)
		{
			ReciveMessageErrorEvent?.Invoke(invalidDevice);
		}

	}
}
