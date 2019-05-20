using EventCore.Samples.Ecommerce.Projections;
using Microsoft.EntityFrameworkCore;
using System;

namespace EventCore.Samples.Ecommerce.ServiceApi.Infrastructure
{
	public class ProjectorDbContextScopeFactory<TContext> : IDbContextScopeFactory<TContext> where TContext : DbContext
	{
		public IDbContextScope<TContext> Create() => null;
	}
}
