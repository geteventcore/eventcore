using Microsoft.EntityFrameworkCore;

namespace EventCore.Samples.Ecommerce.Shared
{
	public interface IDbContextScopeFactory<TContext> where TContext : DbContext
	{
		IDbContextScope<TContext> Create();
	}
}
