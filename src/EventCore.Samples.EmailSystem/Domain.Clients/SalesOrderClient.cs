using EventCore.Samples.EmailSystem.Domain.SalesOrder.Commands;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.Clients
{
	public class SalesOrderClient : HttpDomainClient,
		IExecuteClientCommand<RaiseSalesOrderCommand>
	{
		public SalesOrderClient(string baseUrl) : base(baseUrl) { }

		public Task ExecuteAsync(RaiseSalesOrderCommand command) => PostCommandAsync(command._CommandName, command);
	}
}
