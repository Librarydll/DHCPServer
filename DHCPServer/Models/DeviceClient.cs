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
		private readonly Device _device = null;
		private readonly HttpClient client = null;
		private readonly string _url = null;
		public DeviceClient(Device device)
		{
			if (device == null) throw new ArgumentNullException("device should not be null");
			if (device.IPAddress == null) throw new ArgumentNullException("device IPAddress should not be null");
			
			_device = device;
			client = new HttpClient
			{
				Timeout = new TimeSpan(0, 0, 10)
			};
			_url = "http://" + _device.IPAddress;
		}

		public async Task ListenAsync(CancellationToken token)
		{
			HttpResponseMessage response = null;

			while (true)
			{
				try
				{
					await Task.Delay(2000);

					response = await client.GetAsync(_url, token);
					//response.EnsureSuccessStatusCode();
					if (response.IsSuccessStatusCode)
					{
						string responseBody = await response.Content.ReadAsStringAsync();
						ReciveMessageRaise(responseBody, _device);
					}
					if (token.IsCancellationRequested)
					{
						token.ThrowIfCancellationRequested();
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
					ReciveMessageErroRaise(_device);
				}
				catch (Exception ex)
				{
					ReciveMessageErroRaise(_device);
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
				var result = new RoomInfo(room, device.IPAddress);
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
