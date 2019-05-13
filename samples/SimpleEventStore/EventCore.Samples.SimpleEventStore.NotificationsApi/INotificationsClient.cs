using System.Threading.Tasks;

namespace EventCore.Samples.SimpleEventStore.NotificationsApi
{
	public interface INotificationsClient
	{
		Task ReceiveClientNotification(long globalPosition);
	}
}
