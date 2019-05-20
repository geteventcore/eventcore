using EventCore.Samples.Ecommerce.Projections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCore.Samples.Ecommerce.ServiceApi.Infrastructure
{
	public class ProjectorDbContextScopeFactory<TContext> : IDbContextScopeFactory<TContext> where TContext : DbContext
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public ProjectorDbContextScopeFactory(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
		}

		public IDbContextScope<TContext> Create() => new ProjectorDbContextScope<TContext>(_serviceScopeFactory.CreateScope());
	}
}
