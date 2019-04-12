using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain
{
	public class GenericAggregateRootState : IAggregateRootState
	{
		public long? StreamPositionCheckpoint => throw new NotImplementedException();

		public Task ApplyGenericEventAsync(BusinessEvent e, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
		
		public Task HydrateAsync(IStreamClient streamClient, string streamId)
		{
			throw new NotImplementedException();
		}
	}
}
