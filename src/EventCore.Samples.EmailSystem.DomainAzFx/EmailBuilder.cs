using EventCore.Samples.EmailSystem.Domain.EmailBuilder;
using EventCore.Samples.EmailSystem.Domain.EmailBuilder.Commands;
using EventCore.Samples.EmailSystem.DomainAzFx.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.DomainAzFx
{
	public static class EmailBuilder
	{
		[FunctionName("BuildSalesOrderEmail")]
		public static Task<IActionResult> BuildSalesOrderEmailAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
		{
			return new CommandProcessor<EmailBuilderAggregate, BuildSalesOrderEmailCommand>().TryProcessCommandAsync(req, log);
		}
	}
}
