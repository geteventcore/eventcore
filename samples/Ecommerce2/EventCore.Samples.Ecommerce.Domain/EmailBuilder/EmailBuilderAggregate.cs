using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder
{
	public class EmailBuilderAggregate : AggregateRoot<EmailBuilderState>,
		IHandleCommand<EmailBuilderState, BuildSalesOrderEmailCommand>
	{
		public const string NAME = "EmailBuilder";

		public EmailBuilderAggregate(AggregateRootDependencies<EmailBuilderState> dependencies) : base(dependencies, null, NAME)
		{
		}

		public Task<ICommandResult> HandleCommandAsync(EmailBuilderState s, BuildSalesOrderEmailCommand c, CancellationToken ct)
		{
			// if (state.Message != null) return CommandResult.FromErrorIAsync("Duplicate email id.");
			// return CommandResult.FromEventIAsync(new EmailEnqueuedEvent(BusinessEventMetadata.FromCausalId(c.Metadata.CommandId), c.EmailId));
			return CommandResult.FromEventsIAsync(new IBusinessEvent[] { });

		}
	}
}
