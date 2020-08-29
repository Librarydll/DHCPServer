using DHCPServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DHCPServer.Services
{

	public enum DeviceResponseStatus
	{
		Success,
		Fail
	}
	public class ClientService : IClientService, IDisposable
	{
		public event Action<RoomInfo, DeviceResponseStatus> ReciveMessageEvent;
		public event Action<Device> ReciveMessageErrorEvent;
		private HttpClient client;

		public ICollection<RoomInfo> RoomInfos { get; } = new List<RoomInfo>();

		public ClientService()
		{
			client = new HttpClient
			{
				Timeout = new TimeSpan(0, 0, 10)
			};
		}


		public async Task TryRecieve(CancellationToken token, IEnumerable<Device> devices)
		{
			Device invalidDevice = null;
			HttpResponseMessage response = null;
			while (true)
			{
				try
				{
					await Task.Delay(2000);
					token.ThrowIfCancellationRequested();
					foreach (var device in devices)
					{
						if (token.IsCancellationRequested)
						{
							token.ThrowIfCancellationRequested();
						}
						invalidDevice = device;
						var uri = "http://" + device.IPAddress;
						response = await client.GetAsync(uri, token);
						if (response.IsSuccessStatusCode)
						{
							string responseBody = await response.Content.ReadAsStringAsync();
							ReciveMessageRaise(responseBody, device);
						}
						
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
					ReciveMessageErroRaise(invalidDevice);
				}
				catch (Exception ex)
				{
					ReciveMessageErroRaise(invalidDevice);
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
		public void Dispose()
		{
			if (client != null)
				client = null;
		}
	}
}
