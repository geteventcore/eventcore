using EventCore.AggregateRoots;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.State
{
	public interface ISerializableAggregateRootState : IAggregateRootState
	{
		Task DeserializeInternalStateAsync(byte[] data);
		Task<byte[]> SerializeInternalStateAsync();
	}
}
