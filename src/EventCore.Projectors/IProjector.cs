using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Projectors
{
	public interface IProjector
	{
		Task RunAsync(CancellationToken cancellationToken);
	}
}
