using EventCore.AggregateRoots;
using EventCore.ProcessManagers;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.Commands;
using EventCore.Samples.Ecommerce.Events;
using EventCore.Samples.Ecommerce.ProcessManagers.EmailBuildManagement.Processes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailBuildManagement
{
	public partial class EmailBuildManager :
		IHandleBusinessEvent<SalesOrderRaisedEvent>
	{
		public async Task HandleBusinessEventAsync(string streamId, long position, SalesOrderRaisedEvent e, CancellationToken cancellationToken)
		{
			// Save sales order info to durable state.
			// ...

			// Enqueue process to build email receipt.
			await this.EnqueueProcessExecutionAsync<BuildSalesReceiptEmailProcess>(e.SalesOrderId);
		}
	}
}
