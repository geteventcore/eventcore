using EventCore.ProcessManagers;
using EventCore.Samples.Ecommerce.Domain.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailManagement
{
	public class EmailManager : ProcessManager,
		IHandleBusinessEvent<SalesOrderRaisedEvent>,
		IHandleBusinessEvent<EmailEnqueuedEvent>
	{
		public EmailManager(ProcessManagerDependencies dependencies) : base(dependencies)
		{
			RegisterProcess(new EmailSendProcess());
			RegisterProcess(new EmailBuilderProcess());
		}

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
