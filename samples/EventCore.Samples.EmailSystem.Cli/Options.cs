using CommandLine;

namespace EventCore.Samples.EmailSystem.Cli
{
	public static class Options
	{
		[Verb("generateSalesOrders", HelpText = "Generates and submits requested number of RaiseSalesOrder commands.")]
		public class GenerateSalesOrdersOptions
		{
			[Value(0, MetaName = "count", HelpText = "Number of sales orders to raise.")]
			public int? Count { get; set; } = 1000;
		}

		[Verb("listenSalesOrders", HelpText = "Listens for all sales order related events.")]
		public class ListenSalesOrdersOptions
		{
		}
	}
}
