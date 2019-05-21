using System;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public interface IProcess : IDisposable
	{
		Task ExecuteAsync(string processId);
	}
}
