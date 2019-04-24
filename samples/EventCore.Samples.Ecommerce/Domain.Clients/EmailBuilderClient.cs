using EventCore.Samples.Ecommerce.Domain.EmailBuilder.Commands;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.Clients
{
	public class EmailBuilderClient : HttpDomainClient,
		IExecuteClientCommand<BuildSalesOrderEmailCommand>
	{
		public EmailBuilderClient(string baseUrl, HttpClient httpClient) : base(baseUrl, httpClient)
		{
		}

		public Task ExecuteAsync(BuildSalesOrderEmailCommand command) => PostCommandAsync(command);
	}
}