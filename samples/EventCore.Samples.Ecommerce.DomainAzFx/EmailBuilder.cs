using EventCore.Samples.Ecommerce.Domain.EmailBuilder;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.Commands;
using EventCore.Samples.Ecommerce.DomainAzFx.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.DomainAzFx
{
	public static class EmailBuilder
	{
		[FunctionName("BuildSalesOrderEmail")]
		public static Task<IActionResult> BuildSalesOrderEmailAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
			=> CommandProcessor<EmailBuilderAggregate, BuildSalesOrderEmailCommand>.TryProcessCommandAsync(req, log);
	}
}