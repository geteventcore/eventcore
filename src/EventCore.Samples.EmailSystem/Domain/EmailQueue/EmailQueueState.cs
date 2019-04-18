﻿using EventCore.AggregateRoots.SerializableState;
using EventCore.EventSourcing;
using EventCore.Samples.EmailSystem.Domain.EmailQueue.StateModels;
using EventCore.Samples.EmailSystem.Events;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue
{
	public class EmailQueueState : SerializableAggregateRootState<EmailQueueMessageModel>,
		IApplyBusinessEvent<EmailEnqueuedEvent>
	{
		protected override EmailQueueMessageModel _internalState { get => Message; set => Message = value; }


		public EmailQueueMessageModel Message { get; private set; }

		public EmailQueueState(IBusinessEventResolver resolver, ISerializableAggregateRootStateObjectRepo repo, string regionId, string context, string aggregateRootName, string aggregateRootId) : base(resolver, repo, regionId, context, aggregateRootName, aggregateRootId)
		{
		}

		public Task ApplyBusinessEventAsync(string streamId, long position, EmailEnqueuedEvent e, CancellationToken cancellationToken)
		{
			if (Message != null)
			{
				return Task.CompletedTask;
			}

			Message = new EmailQueueMessageModel(e.EmailId);
			return Task.CompletedTask;
		}
	}
}
