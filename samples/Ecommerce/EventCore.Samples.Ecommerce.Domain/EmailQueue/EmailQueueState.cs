﻿using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Domain.EmailQueue.StateModels;
using EventCore.Samples.Ecommerce.Events;
using EventCore.Samples.Ecommerce.Domain.State;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailQueue
{
	public class EmailQueueState : SerializableAggregateRootState<EmailQueueMessageModel>,
		IApplyBusinessEvent<EmailEnqueuedEvent>
	{
		protected override EmailQueueMessageModel _internalState { get => Message; set => Message = value; }

		public EmailQueueMessageModel Message { get; private set; }

		public EmailQueueState(IBusinessEventResolver resolver) : base(resolver)
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
