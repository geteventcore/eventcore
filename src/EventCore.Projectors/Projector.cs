using EventCore.EventSourcing;
using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Projectors
{
	public abstract class Projector : IProjector,
		ISubscriberEventSorter, ISubscriberEventHandler
	{
		// This can be anything except null or empty.
		// Must have a default way to group events to be handled.
		private const string DEFAULT_PARALLEL_KEY = ".";

		private readonly IStandardLogger _logger;
		private readonly ISubscriber _subscriber;

		public Projector(IStandardLogger logger, ISubscriberFactory subscriberFactory, IStreamClientFactory streamClientFactory, IStreamStateRepo streamStateRepo, IBusinessEventResolver resolver, SubscriberFactoryOptions factoryOptions, IList<SubscriptionStreamId> subscriptionStreamIds)
		{
			_logger = logger;
			_subscriber = subscriberFactory.Create(logger, streamClientFactory, streamStateRepo, resolver, this, this, factoryOptions, subscriptionStreamIds);
		}

		public async Task RunAsync(CancellationToken cancellationToken)
		{
			await _subscriber.SubscribeAsync(cancellationToken);
		}

		public virtual string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent)
		{
			// Default is no parallel key, i.e. all events from the subscription stream
			// will be handled sequentially with no optimization to execute events in parallel.
			// The implementing class should override this is events can be consumed in paralle,
			// for example, projecting events that are from completely separate, unrelated aggregate instances.
			return DEFAULT_PARALLEL_KEY; // Sorting keys may not be null or empty.
		}

		public abstract Task HandleSubscriberEventAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
	}
}
