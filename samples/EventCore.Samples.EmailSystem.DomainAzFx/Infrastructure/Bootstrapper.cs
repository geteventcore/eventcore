using EventCore.Samples.EmailSystem.Domain.EmailQueue;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventCore.Samples.EmailSystem.DomainAzFx.Infrastructure
{
	public static class Bootstrapper
	{
		public static IServiceProvider ConfigureServices()
		{
			var services = new ServiceCollection()
				.AddTransient<EmailQueueAggregate>();

			return services.BuildServiceProvider();
		}
	}
}
