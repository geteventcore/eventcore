using EventCore.ProcessManagers;
using EventCore.Samples.Ecommerce.Events;
using EventCore.Samples.Ecommerce.ProcessManagers.EmailBuildManagement.Processes;
using EventCore.StatefulSubscriber;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailBuildManagement
{
	public partial class EmailBuildManager : ProcessManager
	{
		public EmailBuildManager(ProcessManagerDependencies dependencies, ProcessManagerOptions options) : base(dependencies, options)
		{
			RegisterProcess(new SalesReceiptEmailBuilderProcess());
		}

		public override string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent)
		{
			if (subscriberEvent.IsResolved)
			{
				switch (subscriberEvent.ResolvedEvent)
				{
					case SalesOrderRaisedEvent e: return e.SalesOrderId;
				}
			}

			return base.SortSubscriberEventToParallelKey(subscriberEvent);
		}
	}
}
