using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.DomainApi.Infrastructure
{
	public static class CommandProcessor
	{
		public static async Task<IActionResult> ProcessCommandAsync(IAggregateRoot ar, DomainCommand c)
		{
			var result = await ar.HandleGenericCommandAsync(c, CancellationToken.None);

			if(result.IsSuccess) return new OkResult();
			else return new BadRequestObjectResult(result.ValidationErrors);
		}
	}
}
