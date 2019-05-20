using Microsoft.EntityFrameworkCore;

namespace EventCore.Samples.Ecommerce.Projections
{
	public interface IDbContextScopeFactory<TContext> where TContext : DbContext
	{
		IDbContextScope<TContext> Create();
	}
}
