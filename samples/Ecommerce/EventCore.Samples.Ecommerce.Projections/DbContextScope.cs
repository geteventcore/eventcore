using EventCore.EventSourcing;
using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EventCore.Samples.Ecommerce.Projections
{
	public class DbContextScope<TContext> : IDisposable where TContext : DbContext
	{

		
		public TContext Db {get; private set;}
		
		public DbContextScope()
		{

		}

	
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Db = null;
				}
				disposedValue = true;
			}
		}

		public void Dispose() => Dispose(true);
	}
}
