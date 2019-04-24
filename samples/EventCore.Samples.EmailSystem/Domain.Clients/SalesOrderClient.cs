using EventCore.Samples.EmailSystem.Domain.SalesOrder.Commands;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.Clients
{
	public class SalesOrderClient : HttpDomainClient,
		IExecuteClientCommand<RaiseSalesOrderCommand>
	{
		public SalesOrderClient(string baseUrl, HttpClient httpClient) : base(baseUrl, httpClient)
		{
		}

		public Task ExecuteAsync(RaiseSalesOrderCommand command) => PostCommandAsync(command);
	}
}