using System;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public interface IHaveProcess<TProcess> where TProcess : IProcess
	{
		TProcess CreateProcess();
	}
}
