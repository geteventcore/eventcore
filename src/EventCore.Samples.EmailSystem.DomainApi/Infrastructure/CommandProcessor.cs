using EventCore.AggregateRoots;
using EventCore.Samples.EmailSystem.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.DomainApi.Infrastructure
{
	public static class CommandProcessor
	{
		public static async Task<IActionResult> ProcessCommandAsync(IAggregateRoot ar, DomainCommand c)
		{
			var result = await ar.HandleGenericCommandAsync(c);

			if(result.IsSuccess) return new OkResult();
			else return new BadRequestObjectResult(result.ValidationErrors);
		}
	}
}
