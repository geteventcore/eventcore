using System.Threading.Tasks;

namespace EventCore.Samples.EventStore.NotificationsApi
{
	public interface INotificationsClient
	{
		Task ReceiveClientNotification(long globalPosition);
	}
}
