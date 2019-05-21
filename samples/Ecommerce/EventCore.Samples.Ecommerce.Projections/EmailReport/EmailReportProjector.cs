using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Domain.Events;
using EventCore.Samples.Ecommerce.Projections.EmailReport.EmailReportDb;
using EventCore.Samples.Ecommerce.Shared;
using EventCore.StatefulSubscriber;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.EmailReport
{
	public partial class EmailReportProjector : Projector
	{
		private readonly IDbContextScopeFactory<EmailReportDbContext> _dbScopeFactory;

		public EmailReportProjector(ProjectorDependencies dependencies, IDbContextScopeFactory<EmailReportDbContext> dbScopeFactory) : base(dependencies)
		{
			_dbScopeFactory = dbScopeFactory;
		}

		public override string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent)
		{
			if (subscriberEvent.IsResolved)
			{
				switch (subscriberEvent.ResolvedEvent)
				{
					case EmailEnqueuedEvent e: return e.EmailId.ToString();
				}
			}

			return base.SortSubscriberEventToParallelKey(subscriberEvent);
		}
	}
}
