using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventCore.Utilities;

namespace EventCore.Samples.EventStore.NotificationsApi
{
	public class NotificationsManager
	{
		private SemaphoreSlim _globalPositionMutex = new SemaphoreSlim(1, 1);

		public long? GlobalPosition { get; private set; } // The last known global position.
		public AsyncAutoResetEvent GlobalPositionAdvancedSignal { get; } = new AsyncAutoResetEvent();

		public async Task TryUpdateGlobalPositionAsync(long globalPosition)
		{
			try
			{
				await _globalPositionMutex.WaitAsync();

				if (!GlobalPosition.HasValue || globalPosition > GlobalPosition.Value)
				{
					GlobalPosition = globalPosition;
				}

				GlobalPositionAdvancedSignal.Set();
			}
			finally
			{
				_globalPositionMutex.Release();
			}
		}
	}
}
