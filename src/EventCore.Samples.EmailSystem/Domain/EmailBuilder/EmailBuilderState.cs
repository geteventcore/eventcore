using EventCore.Samples.EmailSystem.Events;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailBuilder
{
	public class EmailBuilderState : NonSerializeableAggregateRootState,
		IApplyBusinessEvent<EmailEnqueuedEvent>
	{
		// This state implementation will use sql database as state store.
		public Task ApplyBusinessEventAsync(EmailEnqueuedEvent e, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
