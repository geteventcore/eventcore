using EventCore.ProcessManagers;
using EventCore.Samples.Ecommerce.Domain.Events;
using EventCore.Samples.Ecommerce.ProcessManagers.EmailManagement.Processes;
using EventCore.StatefulSubscriber;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailManagement
{
	public partial class EmailManager : ProcessManager
	{
		public EmailManager(ProcessManagerDependencies dependencies, ProcessManagerOptions options) : base(dependencies, options)
		{
			RegisterProcess(new EmailSendProcess());
			RegisterProcess(new EmailBuilderProcess());
		}

		public override string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent)
		{
			if (subscriberEvent.IsResolved)
			{
				// switch (subscriberEvent.ResolvedEvent)
				// {
					
				// }
			}

			return base.SortSubscriberEventToParallelKey(subscriberEvent);
		}
	}
}
