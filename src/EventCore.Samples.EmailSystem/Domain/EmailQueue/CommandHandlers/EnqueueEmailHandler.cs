using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.EmailSystem.Domain.EmailQueue.Commands;
using EventCore.Samples.EmailSystem.Events;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue.CommandHandlers
{
	public class EnqueueEmailHandler : EmailQueueCommandHandler<EnqueueEmailCommand>
	{
		public override Task<ICommandValidationResult> ValidateCommandAsync(EmailQueueState state, EnqueueEmailCommand command)
		{
			if (state.Message != null) return CommandValidationResult.FromErrorAsync("Duplicate email id.");
			else return CommandValidationResult.FromValidAsync();
		}

		public override Task<ICommandEventsResult> ProcessCommandAsync(EmailQueueState state, EnqueueEmailCommand command)
		{
			return CommandEventsResult.FromEventAsync(new EmailEnqueuedEvent(BusinessEventMetadata.FromCausalId(command.Metadata.CommandId), command.EmailId));
		}
	}
}
