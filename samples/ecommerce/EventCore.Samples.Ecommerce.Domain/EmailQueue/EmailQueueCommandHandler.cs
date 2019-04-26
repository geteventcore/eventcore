using EventCore.AggregateRoots;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailQueue
{
	public abstract class EmailQueueCommandHandler<TCommand> : ICommandHandler<EmailQueueState, TCommand>
		where TCommand : EmailQueueCommand
	{
		public EmailQueueCommandHandler() { }
		public abstract Task<ICommandEventsResult> ProcessCommandAsync(EmailQueueState state, TCommand c);
		public abstract Task<ICommandValidationResult> ValidateStateAsync(EmailQueueState state, TCommand c);
	}
}
