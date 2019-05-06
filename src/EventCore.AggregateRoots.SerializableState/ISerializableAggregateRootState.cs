using System.Threading.Tasks;

namespace EventCore.AggregateRoots.SerializableState
{
	public interface ISerializableAggregateRootState : IAggregateRootState
	{
		Task DeserializeInternalStateAsync(byte[] data);
		Task<byte[]> SerializeInternalStateAsync();
	}
}
