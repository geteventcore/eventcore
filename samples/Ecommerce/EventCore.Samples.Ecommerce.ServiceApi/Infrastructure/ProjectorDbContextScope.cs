using EventCore.Samples.Ecommerce.Projections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCore.Samples.Ecommerce.ServiceApi.Infrastructure
{
	public class ProjectorDbContextScope<TContext> : IDbContextScope<TContext> where TContext : DbContext
	{
		private readonly IServiceScope _serviceScope;

		public TContext Db { get; private set; }

		public ProjectorDbContextScope(IServiceScope serviceScope)
		{
			_serviceScope = serviceScope;
			Db = _serviceScope.ServiceProvider.GetRequiredService<TContext>();
		}

		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Db = null;
					_serviceScope.Dispose();
				}
				disposedValue = true;
			}
		}

		public void Dispose() => Dispose(true);
	}
}
