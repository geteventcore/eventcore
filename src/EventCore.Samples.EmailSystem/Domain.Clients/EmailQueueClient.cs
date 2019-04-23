using EventCore.Samples.EmailSystem.Domain.EmailQueue.Commands;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.Clients
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