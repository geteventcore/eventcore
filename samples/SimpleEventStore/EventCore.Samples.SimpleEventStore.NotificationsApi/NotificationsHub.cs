using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.SimpleEventStore.NotificationsApi
{
	public class NotificationsHub : Hub<INotificationsClient>
	{
		private readonly IServiceProvider _serviceProvider;

		public NotificationsHub(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public Task SendClientNotification(long globalPosition) => Clients.All.ReceiveClientNotification(globalPosition);
		
		public async Task ReceiveCommitNotification(long globalPosition)
		{
			var q = _serviceProvider.GetRequiredService<NotificationsManager>();
			await q.TryUpdateGlobalPositionAsync(globalPosition);
		}
	}
}
