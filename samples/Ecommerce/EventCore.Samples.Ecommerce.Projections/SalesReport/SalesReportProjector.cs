using EventCore.Projectors;
using EventCore.StatefulSubscriber;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.SalesReport
{
	public class SalesReportProjector : Projector
	{
		public SalesReportProjector(ProjectorBaseDependencies dependencies) : base(dependencies.Logger, dependencies.SubscriberFactory, dependencies.StreamClientFactory, dependencies.StreamStateRepo, dependencies.EventResolver, dependencies.SubscriberFactoryOptions, dependencies.SubscriptionStreamIds)
		{
		}

		public override Task HandleSubscriberEventAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
