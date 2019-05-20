using Microsoft.EntityFrameworkCore;
using System;

namespace EventCore.Samples.Ecommerce.Projections
{
	public interface IDbContextScope<TContext> : IDisposable where TContext : DbContext
	{
		TContext Db { get; }
	}
}
