using DHCPServer.Domain.Models;
using DHCPServer.Services;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DHCPServer.Models
{
	public class DeviceClient<TDevice,TRoom> : IDisposable 
		where TDevice : Device
		where TRoom   :	RoomInfo , new()
	{
		public event Action<TRoom> ReciveMessageOnSuccessEvent;
		public event Action ReciveMessageOnErrorEvent;
		public event Action EnableDeviceEvent;
		public TDevice ActiveDevice { get; set; }
		private readonly HttpClient client = null;
		private readonly string _url = null;
		private readonly IParser<TRoom> _parser =null;
		public bool IsInvalid => _countRequestForDisableInvaid != 0;
		private int _countRequestForDisableInvaid = 0;
		private bool loop = true;
		private bool _disposed = false;
		public DeviceClient(TDevice device)
		{
			if (device == null) throw new ArgumentNullException("device should not be null");
			if (device?.IPAddress == null) throw new ArgumentNullException("device IPAddress should not be null");
			ActiveDevice = device;
			_parser = new Parser<TRoom>();
			client = new HttpClient
			{
				Timeout = new TimeSpan(0, 0, 5)
			};
			_url = "http://" + device.IPAddress;
		}



		public async Task ListenAsync(CancellationToken token)
		{
			HttpResponseMessage response = null;

			while (loop)
			{
				try
				{
					await Task.Delay(5000).ConfigureAwait(false);

					if (!IsInvalid)
					{
						response = await client.GetAsync(_url, token);
						if (response.IsSuccessStatusCode)
						{
							string responseBody = await response.Content.ReadAsStringAsync();
							RaiseOnSuccess(responseBody);
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
							EnableDeviceEvent?.Invoke();

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
				catch (HttpRequestException)
				{
					_countRequestForDisableInvaid = 2;
					RaiseOnError();
				}
				catch (Exception e)
				{
					Log.Logger.Error("Exception message {0}", e.Message);
					Log.Logger.Error("Exception inner message {0}", e?.InnerException?.Message);
					_countRequestForDisableInvaid = 2;
					RaiseOnError();
				}
				finally
				{
					if (response != null) response.Dispose();
				}
			}
		}

		private void RaiseOnSuccess(string responseBody)
		{			
			var room = _parser.Parse(responseBody);
			ReciveMessageOnSuccessEvent?.Invoke(room);

		}

		private void RaiseOnError()
		{
			ReciveMessageOnErrorEvent?.Invoke();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);

		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				client?.Dispose();
				loop = false;
			}
			_disposed = true;
		}
	}
}
