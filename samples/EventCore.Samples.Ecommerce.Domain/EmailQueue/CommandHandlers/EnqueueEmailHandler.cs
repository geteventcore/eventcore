using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Domain.EmailQueue.Commands;
using EventCore.Samples.Ecommerce.Events;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailQueue.CommandHandlers
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
				BusinessEventMetadata.FromCausalId(c._Metadata.CommandId),
				c.EmailId, c.FromAddress, c.ToAddress, c.Subject, c.Body, c.IsHtml
			);
			return CommandEventsResult.FromEventAsync(e);
		}
	}
}
