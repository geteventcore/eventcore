using EventCore.EventSourcing;
using EventCore.Projectors;
using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.SalesReport
{
	public class SalesReportProjector : Projector
	{
		public SalesReportProjector(IStandardLogger logger, ISubscriberFactory subscriberFactory, IStreamClientFactory streamClientFactory, IStreamStateRepo streamStateRepo, IBusinessEventResolver resolver, SubscriberFactoryOptions factoryOptions, IList<SubscriptionStreamId> subscriptionStreamIds) : base(logger, subscriberFactory, streamClientFactory, streamStateRepo, resolver, factoryOptions, subscriptionStreamIds)
		{
		}

		public override Task HandleSubscriberEventAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
