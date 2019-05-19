using System.Collections.Generic;
using EventCore.EventSourcing;
using EventCore.Utilities;

namespace EventCore.StatefulSubscriber
{
	public interface ISubscriberFactory
	{
		ISubscriber Create(IStandardLogger logger, IStreamClientFactory streamClientFactory, IStreamStateRepo streamStateRepo, IBusinessEventResolver resolver, ISubscriberEventSorter sorter, ISubscriberEventHandler handler, SubscriberFactoryOptions factoryOptions, IList<SubscriptionStreamId> subscriptionStreamIds);
	}
}
