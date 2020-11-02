﻿using DHCPServer.Domain.Models;
using DHCPServer.Models.Enums;
using DHCPServer.Services;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DHCPServer.Models
{
	public class DeviceClient:IDisposable
	{
		public event Action<RoomInfo, DeviceResponseStatus> ReciveMessageEvent;
		public event Action<ActiveDevice> ReciveMessageErrorEvent;
		public event Action<ActiveDevice> EnableDeviceEvent;
		public ActiveDevice ActiveDevice { get; set; }
		private readonly HttpClient client = null;
		private readonly string _url = null;
		public bool IsInvalid => _countRequestForDisableInvaid!=0;
		private int _countRequestForDisableInvaid = 0;
		private bool loop = true;
		private bool disposed = false;
		public DeviceClient(Device device)
		{
			if (device == null) throw new ArgumentNullException("device should not be null");
			if (device?.IPAddress == null) throw new ArgumentNullException("device IPAddress should not be null");
			if(device.ActiveDevice==null) throw new ArgumentNullException("ActiveDevice should not be null");
			ActiveDevice = device.ActiveDevice;
			client = new HttpClient
			{
				Timeout = new TimeSpan(0, 0, 5)
			};
			_url = "http://" + device.IPAddress;
		}

		public DeviceClient(ActiveDevice activeDevice)
		{
			if(activeDevice==null) throw new ArgumentNullException("ActiveDevice should not be null");
			if (activeDevice?.Device?.IPAddress == null) throw new ArgumentNullException("device IPAddress should not be null");
			ActiveDevice = activeDevice;
			client = new HttpClient
			{
				Timeout = new TimeSpan(0, 0, 5)
			};
			_url = "http://" + activeDevice.Device.IPAddress;
		}

		public async Task ListenAsync(CancellationToken token)
		{
			HttpResponseMessage response = null;

			while (loop)
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
							ReciveMessageRaise(responseBody, ActiveDevice);
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
							EnableDeviceEvent?.Invoke(ActiveDevice);

					}

				}
				catch (InvalidOperationException e)
				{
					Log.Logger.Error("InvalidOperationException message {0}", e.Message);
					Log.Logger.Error("InvalidOperationException inner message {0}", e?.InnerException?.Message);
				}
				catch (ArgumentException e)
				{
					Log.Logger.Error("ArgumentException message {0}", e.Message);
					Log.Logger.Error("ArgumentException inner message {0}", e?.InnerException.Message);
				}
				catch (HttpRequestException e)
				{
				//	Log.Logger.Error("HttpRequestException message {0}", e.Message);
				//	Log.Logger.Error("HttpRequestException inner message {0}", e?.InnerException.Message);
					_countRequestForDisableInvaid = 2;
					ReciveMessageErroRaise(ActiveDevice);
				}
				catch (Exception e)
				{
					Log.Logger.Error("Exception message {0}", e.Message);
					Log.Logger.Error("Exception inner message {0}", e?.InnerException?.Message);
					_countRequestForDisableInvaid = 2;
					ReciveMessageErroRaise(ActiveDevice);
				}
				finally
				{
					if (response != null) response.Dispose();
				} 
			}
		}

		private void ReciveMessageRaise(string responseBody, ActiveDevice device, DeviceResponseStatus status = DeviceResponseStatus.Success)
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

		private void ReciveMessageErroRaise(ActiveDevice invalidDevice)
		{
			ReciveMessageErrorEvent?.Invoke(invalidDevice);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);

		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed&& disposing)
			{
				client?.Dispose();
				loop = false;
			}
		}
	}
}
