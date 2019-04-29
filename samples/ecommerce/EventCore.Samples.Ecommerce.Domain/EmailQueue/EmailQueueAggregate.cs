using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Domain.EmailQueue.Commands;
using EventCore.Samples.Ecommerce.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailQueue
{
	public class EmailQueueAggregate : AggregateRoot<EmailQueueState>,
		IHandleCommand<EmailQueueState, EnqueueEmailCommand>
	{
		public const string NAME = "EmailQueue";

		public EmailQueueAggregate(AggregateRootDependencies<EmailQueueState> dependencies) : base(dependencies, null, NAME)
		{
		}

		public Task<ICommandResult> HandleCommandAsync(EmailQueueState s, EnqueueEmailCommand c, CancellationToken ct)
		{
			if (s.Message != null) return CommandResult.FromErrorIAsync("Duplicate email id.");

			var e = new EmailEnqueuedEvent(
				BusinessEventMetadata.FromCausalId(c._Metadata.CommandId),
				c.EmailId, c.FromAddress, c.ToAddress, c.Subject, c.Body, c.IsHtml
			);
			return CommandResult.FromEventIAsync(e);
		}
	}
}
