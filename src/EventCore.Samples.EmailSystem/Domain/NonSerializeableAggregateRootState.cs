using EventCore.EventSourcing;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain
{
	public abstract class NonSerializeableAggregateRootState : BaseAggregateRootState
	{
		public override bool SupportsSerialization { get => false; }

		public override Task HydrateAsync(IStreamClient streamClient, string streamId)
		{
			throw new NotImplementedException();
		}

		public override Task<string> SerializeAsync() => throw new NotImplementedException();
	}
}
