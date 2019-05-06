using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder
{
	public partial class EmailBuilderState :
		IApplyBusinessEvent<EmailEnqueuedEvent>
	{
		public Task ApplyBusinessEventAsync(string streamId, long position, EmailEnqueuedEvent e, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}