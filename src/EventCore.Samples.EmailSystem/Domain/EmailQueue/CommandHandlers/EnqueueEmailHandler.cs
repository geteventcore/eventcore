using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.EmailSystem.Domain.EmailQueue.Commands;
using EventCore.Samples.EmailSystem.Events;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue.CommandHandlers
{
	public class EnqueueEmailHandler : EmailQueueCommandHandler<EnqueueEmailCommand>
	{
		public override Task<IHandledCommandResult> HandleAsync(EmailQueueState state, EnqueueEmailCommand command)
		{
			if(state.Message != null)
				return HandledCommandResult.FromErrorAsync("Duplicate email id.");

			return HandledCommandResult.FromEventAsync(new EmailEnqueuedEvent(BusinessEventMetadata.FromCausalId(command.Metadata.CommandId), command.EmailId));
		}
	}
}
