using EventCore.AggregateRoots;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.State
{
	public interface IDatabaseAggregateRootState : IAggregateRootState
	{
		Task SaveChangesAsync(CancellationToken cancellationToken);
	}
}
