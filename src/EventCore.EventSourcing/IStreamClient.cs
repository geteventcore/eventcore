using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.EventSourcing
{
	public interface IStreamClient : IDisposable
	{
		long FirstPositionInStream { get; }
		Task<CommitResult> CommitEventsToStreamAsync(string streamId, long? expectedLastPosition, IEnumerable<CommitEvent> events);
		Task LoadStreamEventsAsync(string streamId, long fromPosition, Func<StreamEvent, Task> receiverAsync, CancellationToken cancellationToken);
		Task SubscribeToStreamAsync(string streamId, long fromPosition, Func<StreamEvent, Task> receiverAsync, CancellationToken cancellationToken);
		Task<long?> GetLastPositionInStreamAsync(string streamId);
	}
}
