using EventCore.EventSourcing;
using EventCore.Utilities;
using System.Collections.Generic;

namespace EventCore.StatefulSubscriber
{
	// Convenience class for creating all the supporting classes required for a subscriber.
	public class SubscriberFactory : ISubscriberFactory
	{
		public ISubscriber Create(IStandardLogger logger, IStreamClientFactory streamClientFactory, IStreamStateRepo streamStateRepo, IBusinessEventResolver resolver, ISubscriberEventSorter sorter, ISubscriberEventHandler handler, SubscriberFactoryOptions factoryOptions, IList<SubscriptionStreamId> subscriptionStreamIds)
		{
			var handlingManagerAwaiter = new HandlingManagerAwaiter(factoryOptions.MaxParallelHandlerExecutions);
			var handlingQueue = new HandlingQueue(new QueueAwaiter(), factoryOptions.MaxHandlingQueuesSharedSize);
			var handlerRunner = new HandlingManagerHandlerRunner(logger, handlingManagerAwaiter, streamStateRepo, handler);
			var handlerTasks = new HandlingManagerTaskCollection();
			var handlingManager = new HandlingManager(logger, handlingManagerAwaiter, streamStateRepo, handlingQueue, handlerRunner, handlerTasks);

			var sortingQueue = new SortingQueue(new QueueAwaiter(), factoryOptions.MaxSortingQueueSize);
			var sortingManager = new SortingManager(logger, sortingQueue, sorter, handlingManager);

			var resolutionQueue = new ResolutionQueue(new QueueAwaiter(), factoryOptions.MaxResolutionQueueSize);
			var resolutionManager = new ResolutionManager(logger, resolver, streamStateRepo, resolutionQueue, sortingManager);

			var listener = new SubscriptionListener(logger, streamClientFactory, resolutionManager);

			var subscriberOptions = new SubscriberOptions(subscriptionStreamIds);

			return new Subscriber(logger, streamClientFactory, listener, resolutionManager, sortingManager, handlingManager, streamStateRepo, subscriberOptions);
		}
	}
}
