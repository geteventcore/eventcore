using EventCore.EventSourcing;
using EventCore.Samples.EventStore.StreamDb;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EventStore.Client
{
	public class StreamClient : IStreamClient
	{
		private readonly IStandardLogger _logger;
		private readonly StreamDbContext _db;

		public long FirstPositionInStream => 1;

		public StreamClient(IStandardLogger logger, StreamDbContext db)
		{
			_logger = logger;
			_db = db;
		}

		public Task<CommitResult> CommitEventsToStreamAsync(string streamId, long? expectedLastPosition, IEnumerable<CommitEvent> events)
		{
			return Task.FromResult(CommitResult.Success);
		}

		public Task<long?> GetLastPositionInStreamAsync(string streamId)
		{
			throw new NotImplementedException();
		}

		public Task LoadStreamEventsAsync(string streamId, long fromPosition, Func<StreamEvent, Task> receiverAsync, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task SubscribeToStreamAsync(string streamId, long fromPosition, Func<StreamEvent, Task> receiverAsync, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		// Stream client uses db context, expected to be disposed by caller (via dependency injection, etc.)
		public void Dispose() {}
	}
}
