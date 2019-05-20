using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Projections;
using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EventCore.Samples.Ecommerce.ServiceApi.Infrastructure
{
	public class ProjectorDbContextScope<TContext> : IDbContextScope<TContext> where TContext : DbContext
	{
		public TContext Db {get; private set;}
		
		public ProjectorDbContextScope()
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
