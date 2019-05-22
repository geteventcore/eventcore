using EventCore.ProcessManagers;
using EventCore.Samples.Ecommerce.Events;
using EventCore.Samples.Ecommerce.ProcessManagers.EmailManagement.Processes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailManagement
{
	public partial class EmailManager :
		IHandleBusinessEvent<SalesOrderRaisedEvent>,
		IHandleBusinessEvent<EmailEnqueuedEvent>
	{
		public Task HandleBusinessEventAsync(string streamId, long position, SalesOrderRaisedEvent e, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task HandleBusinessEventAsync(string streamId, long position, EmailEnqueuedEvent e, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
