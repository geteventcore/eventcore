using System.Threading;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public interface IProcessManager
	{
		Task RunAsync(CancellationToken cancellationToken);
	}
}
