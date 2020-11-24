using DHCPServer.Domain.Models;
using DHCPServer.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DHCPServer.Models.Infrastructure.Common
{
	public class RoomLineBase<TDevice, TRoom> : BaseEntity ,IDisposable
		where TDevice : Device
		where TRoom : RoomInfo,new()
	{
		public event Action<TDevice, TRoom> AddToCollectionEvent;

		protected CancellationTokenSource _tokenSource = null;
		protected  DispatcherTimer _timer;

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

		public virtual void OnCollectionAdded()
		{
			AddToCollectionEvent?.Invoke(ActiveDevice, RoomInfo);
		}

		public virtual async Task InitializeDeviceAsync()
		{
			await DeviceClient.ListenAsync(_tokenSource.Token);
		}

		protected virtual void ReciveMessageOnValidEventHandler()
		{
			SetInvalid(false);
		}

		protected virtual void ReciveMessageOnErrorEventHandler()
		{
			SetInvalid(true);
		}

		protected virtual void ReciveMessageEventHandler(TRoom room)
		{
			
		}

		public virtual void SetInvalid(bool value)
		{
			IsInvalid = value;
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
				DeviceClient?.Dispose();
				_timer?.Stop();
				_timer = null;
			}
			_disposed = true;
		}
	}
}
