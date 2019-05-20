using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Domain.Events;
using EventCore.Samples.Ecommerce.Projections.EmailQueue.EmailQueueDb;
using EventCore.StatefulSubscriber;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.EmailQueue
{
	public partial class EmailQueueProjector : Projector
	{
		private readonly IDbContextScopeFactory<EmailQueueDbContext> _dbScopeFactory;

		public EmailQueueProjector(ProjectorBaseDependencies dependencies, IDbContextScopeFactory<EmailQueueDbContext> dbScopeFactory) : base(dependencies.Logger, dependencies.SubscriberFactory, dependencies.StreamClientFactory, dependencies.StreamStateRepo, dependencies.EventResolver, dependencies.SubscriberFactoryOptions, dependencies.SubscriptionStreamIds)
		{
			_dbScopeFactory = dbScopeFactory;
		}

		public override Task HandleSubscriberEventAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
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
