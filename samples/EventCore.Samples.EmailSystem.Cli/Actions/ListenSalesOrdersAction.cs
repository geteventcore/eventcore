using EventCore.AggregateRoots;
using EventCore.Samples.EmailSystem.Domain.Clients;
using EventCore.Samples.EmailSystem.Domain.SalesOrder.Commands;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Cli.Actions
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
