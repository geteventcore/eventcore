using EventCore.AggregateRoots;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailBuilder
{
	public abstract class EmailBuilderCommandHandler<TCommand> : ICommandHandler<EmailBuilderState, TCommand>
		where TCommand : EmailBuilderCommand
	{
		public EmailBuilderCommandHandler() { }
		public abstract Task<ICommandEventsResult> ProcessCommandAsync(EmailBuilderState state, TCommand command);
		public abstract Task<ICommandValidationResult> ValidateCommandAsync(EmailBuilderState state, TCommand command);
	}
}
