using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.EmailSystem.Domain.EmailBuilder.Commands;
using EventCore.Samples.EmailSystem.Events;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailBuilder.CommandHandlers
{
	public class EnqueueEmailHandler : EmailBuilderCommandHandler<BuildSalesOrderEmailCommand>
	{
		public override Task<ICommandValidationResult> ValidateForStateAsync(EmailBuilderState state, BuildSalesOrderEmailCommand c)
		{
			// if (state.Message != null) return CommandValidationResult.FromErrorAsync("Duplicate email id.");
			// else return CommandValidationResult.FromValidAsync();
			return CommandValidationResult.FromValidAsync();
		}

		public override Task<ICommandEventsResult> ProcessCommandAsync(EmailBuilderState state, BuildSalesOrderEmailCommand c)
		{
			// return CommandEventsResult.FromEventAsync(new EmailEnqueuedEvent(BusinessEventMetadata.FromCausalId(c.Metadata.CommandId), c.EmailId));
			return CommandEventsResult.FromEventsAsync(new BusinessEvent[] { });
		}
	}
}
