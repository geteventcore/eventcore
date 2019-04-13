using EventCore.Samples.EmailSystem.Domain.EmailQueue.StateModels;
using EventCore.Samples.EmailSystem.Events;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue
{
	public class EmailQueueState : SerializeableAggregateRootState,
		IApplyBusinessEvent<EmailEnqueuedEvent>
	{
		public EmailQueueMessage Message { get; private set; }

		public Task ApplyBusinessEventAsync(EmailEnqueuedEvent e, CancellationToken cancellationToken)
		{
			if (Message != null)
			{
				return Task.CompletedTask;
			}

			Message = new EmailQueueMessage(e.EmailId);
			return Task.CompletedTask;
		}
	}
}
