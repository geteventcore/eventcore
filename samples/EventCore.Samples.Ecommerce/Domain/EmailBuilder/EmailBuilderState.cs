using EventCore.AggregateRoots;
using EventCore.AggregateRoots.EntityFrameworkState;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.StateModels;
using EventCore.Samples.Ecommerce.Events;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder
{
	public class EmailBuilderState : DbContextAggregateRootState<EmailBuilderDbContext>,
		IApplyBusinessEvent<EmailEnqueuedEvent>
	{
		public EmailBuilderState(AggregateRootStateBusinessEventResolver<EmailBuilderState> resolver, EmailBuilderDbContext db) : base(resolver, db)
		{
		}

		public Task ApplyBusinessEventAsync(string streamId, long position, EmailEnqueuedEvent e, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
