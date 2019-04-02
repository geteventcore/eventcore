using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.EventSourcing
{
	public interface IStreamClient
	{
		long FirstPositionInStream { get; }

		Task<CommitResult> CommitEventsToStreamAsync(string streamId, long? expectedLastPosition, IEnumerable<CommitEvent> events);
		Task LoadStreamEventsAsync(string streamId, long fromPosition, Func<StreamEvent, CancellationToken, Task> receiverAsync, CancellationToken cancellationToken);
		Task<long?> FindLastPositionInStreamAsync(string streamId);
	}
}
