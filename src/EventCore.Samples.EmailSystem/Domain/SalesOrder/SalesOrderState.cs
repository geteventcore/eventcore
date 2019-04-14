using EventCore.Samples.EmailSystem.Domain.SalesOrder.StateModels;
using EventCore.Samples.EmailSystem.Events;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.SalesOrder
{
	public class SalesOrderState : SerializeableAggregateRootState,
		IApplyBusinessEvent<EmailEnqueuedEvent>
	{
		public SalesOrderModel SalesOrder { get; private set; }

		public Task ApplyBusinessEventAsync(EmailEnqueuedEvent e, CancellationToken cancellationToken)
		{
			if (SalesOrder != null)
			{
				return Task.CompletedTask;
			}

			SalesOrder = new StateModels.SalesOrderModel(e.EmailId);
			return Task.CompletedTask;
		}
	}
}
