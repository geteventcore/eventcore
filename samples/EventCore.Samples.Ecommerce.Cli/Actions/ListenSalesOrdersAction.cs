using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain.Clients;
using EventCore.Samples.Ecommerce.Domain.SalesOrder.Commands;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Cli.Actions
{
	public class ListenSalesOrdersAction : IAction
	{
		private readonly Options.ListenSalesOrdersOptions _options;

		public ListenSalesOrdersAction(Options.ListenSalesOrdersOptions options)
		{
			_options = options;
		}

		public async Task RunAsync()
		{
			Console.WriteLine("Listening for sales order events.");
			await Task.Delay(1000);
			Console.WriteLine("Finished listening for sales order events.");
		}
	}
}
