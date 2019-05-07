using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EventCore.Samples.EventStore.NotificationsApi
{
	public class NotificationsHub : Hub
	{
		public Task SendMessage(string user, string message)
		{
			// return Clients.All.SendAsync("ReceiveClientMessage", user, message);
			return null;
		}
	}
}
