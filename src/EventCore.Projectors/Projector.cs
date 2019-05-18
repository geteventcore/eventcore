using System;
using System.Threading;
using System.Threading.Tasks;
using EventCore.StatefulSubscriber;
using EventCore.Utilities;

namespace EventCore.Projectors
{
	public abstract class Projector : IProjector,
		ISubscriberEventSorter, ISubscriberEventHandler
	{
		private readonly IStandardLogger _logger;
		private readonly ISubscriber _subscriber;

		public Projector(IStandardLogger logger, ISubscriber subscriber)
		{
			_logger = logger;
			_subscriber = subscriber;
		}

		public async Task RunAsync(CancellationToken cancellationToken)
		{
			await _subscriber.SubscribeAsync(cancellationToken);
		}

		public virtual string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent)
		{
			return null; // Default is no parallel key, i.e. events will not be sorted for parallel processing.
		}

		public abstract Task HandleSubscriberEventAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
	}
}
