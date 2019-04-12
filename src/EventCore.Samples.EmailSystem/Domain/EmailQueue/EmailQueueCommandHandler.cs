using EventCore.AggregateRoots;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue
{
	public abstract class EmailQueueCommandHandler<TCommand> : ICommandHandler<EmailQueueState, TCommand>
		where TCommand : EmailQueueCommand
	{
		public EmailQueueCommandHandler() { }
		public abstract Task<ICommandEventsResult> ProcessCommandAsync(EmailQueueState state, TCommand command);
		public abstract Task<ICommandValidationResult> ValidateCommandAsync(EmailQueueState state, TCommand command);
	}
}
