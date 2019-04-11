using EventCore.AggregateRoots;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue
{
	public abstract class EmailQueueCommandHandler<TCommand> : ICommandHandler<EmailQueueState, TCommand>
		where TCommand : EmailQueueCommand
	{
		public EmailQueueCommandHandler()
		{
		}

		public abstract Task<IHandledCommandResult> HandleAsync(EmailQueueState state, TCommand command);
	}
}
