using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Domain.Events;
using EventCore.Samples.Ecommerce.Projections.SalesReport.SalesReportDb;
using EventCore.Samples.Ecommerce.Shared;
using EventCore.StatefulSubscriber;

namespace EventCore.Samples.Ecommerce.Projections.SalesReport
{
	public partial class SalesReportProjector : Projector
	{
		private readonly IDbContextScopeFactory<SalesReportDbContext> _dbScopeFactory;

		public SalesReportProjector(ProjectorDependencies dependencies, IDbContextScopeFactory<SalesReportDbContext> dbScopeFactory) : base(dependencies)
		{
			_dbScopeFactory = dbScopeFactory;
		}

		public override string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent)
		{
			if (subscriberEvent.IsResolved)
			{
				switch (subscriberEvent.ResolvedEvent)
				{
					case SalesOrderRaisedEvent e: return e.SalesOrderId;
				}
			}

			return base.SortSubscriberEventToParallelKey(subscriberEvent);
		}
	}
}
