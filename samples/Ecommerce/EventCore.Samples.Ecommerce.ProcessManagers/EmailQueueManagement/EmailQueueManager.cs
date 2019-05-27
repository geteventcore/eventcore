using EventCore.ProcessManagers;
using EventCore.Samples.Ecommerce.Events;
using EventCore.Samples.Ecommerce.ProcessManagers.EmailQueueManagement.Processes;
using EventCore.StatefulSubscriber;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailQueueManagement
{
	public partial class EmailQueueManager : ProcessManager
	{
		public EmailQueueManager(ProcessManagerDependencies dependencies, ProcessManagerOptions options) : base(dependencies, options)
		{
			RegisterProcess(new EnqueueEmailProcess());
		}

		public override string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent)
		{
			if (subscriberEvent.IsResolved)
			{
				switch (subscriberEvent.ResolvedEvent)
				{
					case EmailBuiltEvent e: return e.EmailId.ToString();
					case EmailEnqueuedEvent e: return e.EmailId.ToString();
				}
			}

			return base.SortSubscriberEventToParallelKey(subscriberEvent);
		}
	}
}
