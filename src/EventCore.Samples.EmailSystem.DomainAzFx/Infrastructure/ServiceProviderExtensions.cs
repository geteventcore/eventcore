using EventCore.Samples.EmailSystem.Domain.EmailQueue;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventCore.Samples.EmailSystem.DomainAzFx.Infrastructure
{
	public static class ServiceProviderExtensions
	{
		public static EmailQueueAggregate GetEmailQueueAggregate(this IServiceProvider sp) => sp.GetRequiredService<EmailQueueAggregate>();
	}
}
