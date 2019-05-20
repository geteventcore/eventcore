﻿using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Domain.Events;
using EventCore.StatefulSubscriber;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.EmailQueue
{
	public partial class EmailQueueProjector : Projector
	{
		public EmailQueueProjector(ProjectorBaseDependencies dependencies) : base(dependencies.Logger, dependencies.SubscriberFactory, dependencies.StreamClientFactory, dependencies.StreamStateRepo, dependencies.EventResolver, dependencies.SubscriberFactoryOptions, dependencies.SubscriptionStreamIds)
		{
			
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

			return null;
		}
	}
}
