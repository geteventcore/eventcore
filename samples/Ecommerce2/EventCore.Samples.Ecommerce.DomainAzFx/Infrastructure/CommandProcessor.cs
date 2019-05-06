using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.DomainAzFx.Infrastructure
{
	public class CommandProcessor<TAggregateRoot, TCommand>
		where TAggregateRoot : IAggregateRoot
		where TCommand : DomainCommand
	{
		public static async Task<IActionResult> TryProcessCommandAsync(HttpRequest req, ILogger log)
		{
			try
			{
				var sp = Bootstrapper.ConfigureServices();
				ICommandResult result;

				using (var scope = sp.CreateScope())
				{
					var ar = sp.GetRequiredService<TAggregateRoot>();

					using (var reader = new StreamReader(req.Body))
					{
						var body = await reader.ReadToEndAsync();
						var c = (TCommand)JsonConvert.DeserializeObject(body);

						result = await ar.HandleGenericCommandAsync(c, CancellationToken.None);
					}
				}

				if (result.IsSuccess) return new OkResult();
				else return new BadRequestObjectResult(result.Errors);
			}
			catch (Exception ex)
			{
				log.LogError(ex, "Exception while executing command handler function.");
				throw;
			}
		}
	}
}
