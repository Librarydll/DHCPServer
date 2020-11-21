using DHCPServer.Domain.Models;
using DHCPServer.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DHCPServer.Models.Infrastructure.Common
{
	public abstract class RoomLineBase<TDevice, TRoom> : BaseEntity ,IDisposable
		where TDevice : Device
		where TRoom : RoomInfo,new()
	{
		protected CancellationTokenSource _tokenSource = null;

		protected bool _disposed = false;
		private bool _isInvalid;
		public bool IsInvalid
		{
			get { return _isInvalid; }
			set { SetProperty(ref _isInvalid, value); }
		}
		public TRoom RoomInfo { get; set; }
		public TDevice ActiveDevice { get; set; }
		public DeviceClient<TDevice,TRoom> DeviceClient { get; set; }

		
		public RoomLineBase(TDevice device)
		{
			ActiveDevice = device;
			_tokenSource = new CancellationTokenSource();
			DeviceClient = new DeviceClient<TDevice,TRoom>(ActiveDevice);
			RoomInfo = new TRoom();
			DeviceClient.ReciveMessageOnSuccessEvent += ReciveMessageEventHandler;
			DeviceClient.ReciveMessageOnErrorEvent += ReciveMessageOnErrorEventHandler;
			DeviceClient.EnableDeviceEvent += ReciveMessageOnValidEventHandler;
		}

		public virtual async Task InitializeDeviceAsync()
		{
			await DeviceClient.ListenAsync(_tokenSource.Token);
		}

		protected virtual void ReciveMessageOnValidEventHandler(TDevice device)
		{
			SetInvalid(false);
		}

		protected virtual void ReciveMessageOnErrorEventHandler(TDevice device)
		{
			SetInvalid(true);
		}

		protected virtual void ReciveMessageEventHandler(TRoom room)
		{
			
		}

		public abstract void SetInvalid(bool value);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				DeviceClient?.Dispose();
			}
		}
	}
}
