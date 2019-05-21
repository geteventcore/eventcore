using EventCore.EventSourcing;
using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using System.Collections.Generic;

namespace EventCore.ProcessManagers
{
	public class ProcessManagerDependencies
	{
		public readonly IStandardLogger Logger;
		public readonly ISubscriberFactory SubscriberFactory;
		public readonly IStreamClientFactory StreamClientFactory;
		public readonly IStreamStateRepo StreamStateRepo;
		public readonly IBusinessEventResolver EventResolver;
		public readonly SubscriberFactoryOptions SubscriberFactoryOptions;
		public readonly IList<SubscriptionStreamId> SubscriptionStreamIds;
		public readonly IProcessManagerStateRepo ProcessManagerStateRepo;

		public ProcessManagerDependencies(IStandardLogger logger, ISubscriberFactory subscriberFactory, IStreamClientFactory streamClientFactory, IStreamStateRepo streamStateRepo, IBusinessEventResolver eventResolver, SubscriberFactoryOptions subscriberFactoryOptions, IList<SubscriptionStreamId> subscriptionStreamIds, IProcessManagerStateRepo processManagerStateRepo)
		{
			Logger = logger;
			SubscriberFactory = subscriberFactory;
			StreamClientFactory = streamClientFactory;
			StreamStateRepo = streamStateRepo;
			EventResolver = eventResolver;
			SubscriberFactoryOptions = subscriberFactoryOptions;
			SubscriptionStreamIds = subscriptionStreamIds;
			ProcessManagerStateRepo = processManagerStateRepo;
		}
	}
}
