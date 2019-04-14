using EventCore.Samples.EmailSystem.Domain.EmailBuilder.Commands;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.Clients
{
	public class EmailBuilderClient : HttpDomainClient,
		IExecuteClientCommand<BuildSalesOrderEmailCommand>
	{
		public EmailBuilderClient(string baseUrl) : base(baseUrl) { }

		public Task ExecuteAsync(BuildSalesOrderEmailCommand command) => PostCommandAsync(command._CommandName, command);
	}
}
