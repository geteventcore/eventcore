using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Domain.Events;
using EventCore.Samples.Ecommerce.Projections.SalesReport.SalesReportDb;
using EventCore.StatefulSubscriber;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.SalesReport
{
	public partial class SalesReportProjector : Projector
	{
		private readonly IDbContextScopeFactory<SalesReportDbContext> _dbScopeFactory;

		public SalesReportProjector(ProjectorBaseDependencies dependencies, IDbContextScopeFactory<SalesReportDbContext> dbScopeFactory) : base(dependencies.Logger, dependencies.SubscriberFactory, dependencies.StreamClientFactory, dependencies.StreamStateRepo, dependencies.EventResolver, dependencies.SubscriberFactoryOptions, dependencies.SubscriptionStreamIds)
		{
			_dbScopeFactory = dbScopeFactory;
		}

		public override string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent)
		{
			// if (subscriberEvent.IsResolved)
			// {
			// 	switch (subscriberEvent.ResolvedEvent)
			// 	{
			// 		case SalesOrderRaisedEvent e: return e.SalesOrderId;
			// 	}
			// }

			return base.SortSubscriberEventToParallelKey(subscriberEvent);
		}
	}
}
