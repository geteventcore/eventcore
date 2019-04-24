using EventCore.AggregateRoots;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder
{
	public abstract class EmailBuilderCommandHandler<TCommand> : ICommandHandler<EmailBuilderState, TCommand>
		where TCommand : EmailBuilderCommand
	{
		public EmailBuilderCommandHandler() { }
		public abstract Task<ICommandEventsResult> ProcessCommandAsync(EmailBuilderState state, TCommand c);
		public abstract Task<ICommandValidationResult> ValidateForStateAsync(EmailBuilderState state, TCommand c);
	}
}
