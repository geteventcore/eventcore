﻿using EventCore.Samples.EmailSystem.Domain.EmailQueue.StateModels;
using EventCore.Samples.EmailSystem.Events;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue
{
	public class EmailQueueState : SerializeableAggregateRootState,
		IApplyBusinessEvent<EmailEnqueuedEvent>
	{
		public EmailQueueMessageModel Message { get; private set; }

		public Task ApplyBusinessEventAsync(EmailEnqueuedEvent e, CancellationToken cancellationToken)
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
