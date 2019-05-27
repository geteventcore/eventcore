using EventCore.ProcessManagers;
using EventCore.Samples.Ecommerce.Events;
using EventCore.Samples.Ecommerce.ProcessManagers.EmailQueueManagement.Processes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailQueueManagement
{
	public partial class EmailQueueManager :
		IHandleBusinessEvent<EmailBuiltEvent>,
		IHandleBusinessEvent<EmailEnqueuedEvent>
	{
		public Task HandleBusinessEventAsync(string streamId, long position, EmailEnqueuedEvent e, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task HandleBusinessEventAsync(string streamId, long position, EmailBuiltEvent e, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
