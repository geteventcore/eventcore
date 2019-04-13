using EventCore.EventSourcing;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain
{
	public abstract class SerializeableAggregateRootState : BaseAggregateRootState
	{
		public override bool SupportsSerialization { get => true; }

		public override Task HydrateAsync(IStreamClient streamClient, string streamId)
		{
			throw new NotImplementedException();
		}

		public override Task<string> SerializeAsync()
		{
			// serialize w/ json....
			throw new NotImplementedException();
		}
	}
}
