using EventCore.EventSourcing;
using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Domain.Events;
using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.EmailQueue
{
	public partial class EmailQueueProjector :
		IHandleBusinessEvent<EmailEnqueuedEvent>
	{
		public Task HandleBusinessEventAsync(string streamId, long position, EmailEnqueuedEvent e, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
