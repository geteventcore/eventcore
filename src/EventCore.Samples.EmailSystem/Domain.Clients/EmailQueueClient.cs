using EventCore.Samples.EmailSystem.Domain.EmailQueue.Commands;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.Clients
{
	public class EmailQueueClient : HttpDomainClient,
		IExecuteClientCommand<EnqueueEmailCommand>
	{
		public EmailQueueClient(string baseUrl) : base(baseUrl) { }

		public Task ExecuteAsync(EnqueueEmailCommand command) => PostCommandAsync(command._CommandName, command);
	}
}
