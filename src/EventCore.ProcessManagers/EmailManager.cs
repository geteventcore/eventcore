using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public class EmailManager : ProcessManager,
		IHaveProcess<EnqueueEmailProcess>
	{
		public EnqueueEmailProcess CreateProcess()
		{
			return new EnqueueEmailProcess();
		}
	}
}
