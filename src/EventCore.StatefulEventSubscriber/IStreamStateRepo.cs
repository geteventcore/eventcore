using System.Threading;
using System.Threading.Tasks;
using EventCore.Utilities;

namespace EventCore.StatefulEventSubscriber
{
	public interface IStreamStateRepo
	{
		Task SaveStreamStateAsync(string streamId, long lastAttemptedPosition, bool hasError);
		Task<StreamState> LoadStreamStateAsync(string streamId);
		Task ResetStreamStatesAsync();
		Task ClearStreamStateErrorsAsync(CancellationToken cancellationToken);
	}
}
