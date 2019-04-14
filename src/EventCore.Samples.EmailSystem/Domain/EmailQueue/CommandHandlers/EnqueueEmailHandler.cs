using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.EmailSystem.Domain.EmailQueue.Commands;
using EventCore.Samples.EmailSystem.Events;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue.CommandHandlers
{
	public class EnqueueEmailHandler : EmailQueueCommandHandler<EnqueueEmailCommand>
	{
		public override Task<ICommandValidationResult> ValidateForStateAsync(EmailQueueState state, EnqueueEmailCommand c)
		{
			if (state.Message != null) return CommandValidationResult.FromErrorAsync("Duplicate email id.");
			else return CommandValidationResult.FromValidAsync();
		}

		public override Task<ICommandEventsResult> ProcessCommandAsync(EmailQueueState state, EnqueueEmailCommand c)
		{
			var e = new EmailEnqueuedEvent(
				BusinessEventMetadata.FromCausalId(c.Metadata.CommandId),
				c.EmailId, c.FromAddress, c.ToAddress, c.Subject, c.Body, c.IsHtml
			);
			return CommandEventsResult.FromEventAsync(e);
		}
	}
}
