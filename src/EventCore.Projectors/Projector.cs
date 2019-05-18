using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Projectors
{
	public abstract class Projector : IProjector
	{
		public Task RunAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public abstract Task HandleSubscriberEventAsync();

	
	}
}
