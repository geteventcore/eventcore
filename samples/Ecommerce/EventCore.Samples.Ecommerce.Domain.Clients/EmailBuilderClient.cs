using EventCore.Samples.Ecommerce.Domain.EmailBuilder.Commands;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.Clients
{
	public class EmailBuilderClient : HttpDomainClient,
		IExecuteClientCommand<BuildSalesReceiptEmailCommand>
	{
		public EmailBuilderClient(string baseUrl, HttpClient httpClient) : base(baseUrl, httpClient)
		{
		}

		public Task ExecuteAsync(BuildSalesReceiptEmailCommand command) => PostCommandAsync(command);
	}
}