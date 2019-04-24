using EventCore.Samples.Ecommerce.Domain.EmailQueue.Commands;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.Clients
{
	public class EmailQueueClient : HttpDomainClient,
		IExecuteClientCommand<EnqueueEmailCommand>
	{
		public EmailQueueClient(string baseUrl, HttpClient httpClient) : base(baseUrl, httpClient)
		{
		}

		public Task ExecuteAsync(EnqueueEmailCommand command) => PostCommandAsync(command);
	}
}