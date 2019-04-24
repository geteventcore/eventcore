using EventCore.Samples.EmailSystem.Domain.EmailQueue;
using EventCore.Samples.EmailSystem.Domain.EmailQueue.Commands;
using EventCore.Samples.EmailSystem.DomainAzFx.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.DomainAzFx
{
	public static class EmailQueue
	{
		[FunctionName("EnqueueEmail")]
		public static Task<IActionResult> EnqueueEmailAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
			=> CommandProcessor<EmailQueueAggregate, EnqueueEmailCommand>.TryProcessCommandAsync(req, log);
	}
}
