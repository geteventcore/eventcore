using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain.SalesOrder.CommandHandlers;
using EventCore.Samples.Ecommerce.Domain.SalesOrder.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.SalesOrder
{
	public class SalesOrderCommandHandlerFactory : CommandHandlerFactory<SalesOrderState>
	{
		public SalesOrderCommandHandlerFactory()
			: base(new Dictionary<Type, object>()
			{
				{typeof(RaiseSalesOrderCommand), new RaiseSalesOrderHandler()}
			})
		{ }
	}

	public class CommandHandlerFactory<TState> : ICommandHandlerFactory<TState> where TState : IAggregateRootState
	{
		private readonly IDictionary<Type, object> _handlerMap;

		public CommandHandlerFactory(IDictionary<Type, object> handlerMap)
		{
			_handlerMap = handlerMap;
		}

		public ICommandHandler<TState, TCommand> Create<TCommand>() where TCommand : ICommand
		{
			object value;
			if (!_handlerMap.TryGetValue(typeof(TCommand), out value))
			{
				throw new ArgumentException("Command type not supported.");
			}

			var handler = value as ICommandHandler<TState, TCommand>;

			if (handler == null)
			{
				throw new InvalidCastException("Handler type arguments do not match expected types.");
			}

			return handler;
		}
	}
}
