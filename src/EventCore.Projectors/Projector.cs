using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Projectors
{
	public abstract class Projector : IProjector, ISubscriberEventSorter, ISubscriberEventHandler
	{
		// This can be anything except null or empty.
		// Must have a default way to group events to be handled.
		public const string DEFAULT_PARALLEL_KEY = ".";

		protected readonly IStandardLogger _logger;
		protected readonly ISubscriber _subscriber;

		public Projector(ProjectorDependencies dependencies)
		{
			_logger = dependencies.Logger;
			_subscriber = dependencies.SubscriberFactory.Create(dependencies.Logger, dependencies.StreamClientFactory, dependencies.StreamStateRepo, dependencies.EventResolver, this, this, dependencies.SubscriberFactoryOptions, dependencies.SubscriptionStreamIds);
		}

		public virtual async Task RunAsync(CancellationToken cancellationToken)
		{
			await _subscriber.SubscribeAsync(cancellationToken);
		}

		public virtual string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent)
		{
			// Default is no parallel key, i.e. all events from the subscription stream
			// will be handled sequentially with no optimization to execute events in parallel.
			// The implementing class should override this if events can be consumed in parallel,
			// for example, projecting events that are from completely separate aggregate instances,
			// where the parallel key would be the aggregate root id, i.e. the transactional boundary.
			return DEFAULT_PARALLEL_KEY; // Sorting keys may not be null or empty.
		}

		public virtual async Task HandleSubscriberEventAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			var allInterfaces = this.GetType().GetInterfaces();
			
			// Does nothing if no handler - event is ignored.
			if (this.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandleBusinessEvent<>) && x.GetGenericArguments()[0] == subscriberEvent.ResolvedEventType))
			{
				await (Task)this.GetType().InvokeMember("HandleBusinessEventAsync", BindingFlags.InvokeMethod, null, this, new object[] { subscriberEvent.StreamId, subscriberEvent.Position, subscriberEvent.ResolvedEvent, cancellationToken });
			}
		}
	}
}
