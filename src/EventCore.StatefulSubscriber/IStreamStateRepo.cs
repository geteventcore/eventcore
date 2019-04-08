using System.Threading.Tasks;
using EventCore.Utilities;

namespace EventCore.StatefulSubscriber
{
	public interface IStreamStateRepo
	{
		Task SaveStreamStateAsync(string streamId, long lastAttemptedPosition, bool hasError);
		Task<StreamState> LoadStreamStateAsync(string streamId);
	}
}
