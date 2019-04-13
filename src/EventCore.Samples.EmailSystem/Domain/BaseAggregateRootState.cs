using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain
{
	public abstract class BaseAggregateRootState : IAggregateRootState
	{
		public long? StreamPositionCheckpoint { get; protected set; }

		public abstract bool SupportsSerialization { get; }

		public async Task ApplyGenericBusinessEventAsync<TEvent>(TEvent e, CancellationToken cancellationToken)
			where TEvent : BusinessEvent
		{
			await (Task)this.GetType().InvokeMember("ApplyBusinessEventAsync", BindingFlags.InvokeMethod, null, this, new object[] { e, cancellationToken });
		}

		public abstract Task HydrateAsync(IStreamClient streamClient, string streamId);
		public abstract Task<string> SerializeAsync();
	}
}
