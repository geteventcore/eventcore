using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain.Clients;
using EventCore.Samples.Ecommerce.Domain.SalesOrder.Commands;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Cli.Actions
{
	public class GenerateSalesOrdersAction : IAction
	{
		private readonly Options.GenerateSalesOrdersOptions _options;

		public GenerateSalesOrdersAction(Options.GenerateSalesOrdersOptions options)
		{
			_options = options;
		}

		public async Task RunAsync()
		{
			using (var httpClient = new HttpClient())
			{
				var client = new SalesOrderClient(Constants.DOMAIN_API_URL, httpClient);

				for (var i = 1; i <= _options.Count; i++)
				{
					var so = new RaiseSalesOrderCommand(
						new CommandMetadata(Guid.NewGuid().ToString()),
						"SO-" + string.Format(i.ToString(), "0000000000"),
						"Customer " + i,
						"customer." + i + "@fake-email-goes-nowhere-abc.com",
						new Random().Next(1, 100000)
					);

					try
					{
						await client.ExecuteAsync(so);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}

					Console.WriteLine("Raised " + so.SalesOrderId);
				}
			}
		}
	}
}
