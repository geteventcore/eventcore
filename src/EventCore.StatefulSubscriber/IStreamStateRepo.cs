using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface IStreamStateRepo
	{
		Task SaveStreamStateAsync(string streamId, long? lastProcessedPosition, bool hasError);
		Task<StreamState> LoadStreamStateAsync(string streamId);
	}
}
