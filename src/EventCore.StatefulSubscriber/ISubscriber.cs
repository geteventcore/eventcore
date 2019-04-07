using EventCore.EventSourcing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface ISubscriber
	{
		Task SubscribeAsync(IBusinessEventResolver resolver, Func<SubscriberEvent, string> sorter, Func<SubscriberEvent, CancellationToken, Task> handlerAsync, CancellationToken cancellationToken);
	}
}
