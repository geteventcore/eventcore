using Microsoft.EntityFrameworkCore;
using System;

namespace EventCore.Samples.Ecommerce.Projections
{
	public class DbContextFactoryScope<TContext> where TContext : DbContext
	{
		public DbContextScope<TContext> Create() => null;
	}
}
