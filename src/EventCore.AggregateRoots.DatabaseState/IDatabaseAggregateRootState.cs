using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots.DatabaseState
{
	public interface IDatabaseAggregateRootState : IAggregateRootState
	{
		Task SaveChangesAsync(CancellationToken cancellationToken);
	}
}
