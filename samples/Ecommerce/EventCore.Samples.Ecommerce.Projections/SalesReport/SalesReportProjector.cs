using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Projections.SalesReport.SalesReportDb;
using EventCore.StatefulSubscriber;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.SalesReport
{
	public class SalesReportProjector : Projector
	{
		private readonly IDbContextScopeFactory<SalesReportDbContext> _dbScopeFactory;

		public SalesReportProjector(ProjectorBaseDependencies dependencies, IDbContextScopeFactory<SalesReportDbContext> dbScopeFactory) : base(dependencies.Logger, dependencies.SubscriberFactory, dependencies.StreamClientFactory, dependencies.StreamStateRepo, dependencies.EventResolver, dependencies.SubscriberFactoryOptions, dependencies.SubscriptionStreamIds)
		{
			_dbScopeFactory = dbScopeFactory;
		}
	}
}
