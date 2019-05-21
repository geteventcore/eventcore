using System;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public interface IProcess
	{
		Task ExecuteAsync(string processId);
	}
}
