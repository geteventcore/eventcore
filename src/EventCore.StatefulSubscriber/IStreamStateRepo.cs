using System.Threading.Tasks;
using EventCore.Utilities;

namespace EventCore.StatefulSubscriber
{
	public interface IStreamStateRepo
	{
		Task SaveStreamStateAsync(IGenericLogger logger, string streamId, long lastAttemptedPosition, bool hasError);
		Task<StreamState> LoadStreamStateAsync(IGenericLogger logger, string streamId);
	}
}
