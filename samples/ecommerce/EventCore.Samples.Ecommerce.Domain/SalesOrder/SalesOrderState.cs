using EventCore.AggregateRoots;
using EventCore.AggregateRoots.SerializableState;
using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Domain.SalesOrder.StateModels;
using EventCore.Samples.Ecommerce.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.SalesOrder
{
	public class SalesOrderState : SerializableAggregateRootState<SalesOrderModel>,
		IApplyBusinessEvent<EmailEnqueuedEvent>
	{
		protected override SalesOrderModel _internalState { get => SalesOrder; set => SalesOrder = value; }

		public SalesOrderModel SalesOrder { get; private set; }

		public SalesOrderState(IBusinessEventResolver resolver, ISerializableAggregateRootStateObjectRepo repo) : base(resolver, repo)
		{
		}

		public Task ApplyBusinessEventAsync(string streamId, long position, EmailEnqueuedEvent e, CancellationToken cancellationToken)
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
