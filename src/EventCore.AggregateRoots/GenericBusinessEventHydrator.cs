using EventCore.EventSourcing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public class GenericBusinessEventHydrator : IGenericBusinessEventHydrator
	{
		public async Task ApplyGenericBusinessEventAsync(IAggregateRootState state, string streamId, long position, IBusinessEvent e, CancellationToken cancellationToken)
		{
			// Expects IApplyBusinessEvent<TEvent> for the type of event given.
			await (Task)state.GetType().InvokeMember("ApplyBusinessEventAsync", BindingFlags.InvokeMethod, null, state, new object[] { streamId, position, e, cancellationToken });
		}
	}
}
