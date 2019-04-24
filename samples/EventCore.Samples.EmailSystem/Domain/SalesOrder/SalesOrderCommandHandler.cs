using EventCore.AggregateRoots;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.SalesOrder
{
	public abstract class SalesOrderCommandHandler<TCommand> : ICommandHandler<SalesOrderState, TCommand>
		where TCommand : SalesOrderCommand
	{
		public SalesOrderCommandHandler() { }
		public abstract Task<ICommandEventsResult> ProcessCommandAsync(SalesOrderState state, TCommand c);
		public abstract Task<ICommandValidationResult> ValidateForStateAsync(SalesOrderState state, TCommand c);
	}
}
