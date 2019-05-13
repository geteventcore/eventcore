using EventCore.Samples.SimpleEventStore.EventStoreDb.Config;
using EventCore.Samples.SimpleEventStore.EventStoreDb.DbModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCore.Samples.SimpleEventStore.EventStoreDb
{
	public class EventStoreDbContext : DbContext
	{
		public DbSet<SubscriptionFilterDbModel> SubscriptionFilter { get; set; }
		public DbSet<StreamEventDbModel> StreamEvent { get; set; }

		public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new SubscriptionFilterDbModelConfig());
			builder.ApplyConfiguration(new StreamEventDbModelConfig());
		}

		public long? GetMaxGlobalPosition() => GetMaxGlobalPositionInternal(this);
		public long? GetLastStreamPositionByStreamId(string streamId) => GetLastStreamPositionByStreamIdInternal(this, streamId);
		public IEnumerable<StreamEventDbModel> GetStreamEventsByStreamId(string streamId, long minStreamPosition) => GetStreamEventsByStreamIdInternal(this, streamId, minStreamPosition);
		public IEnumerable<StreamEventDbModel> GetStreamEventsByGlobalPosition(string streamId, long minGlobalPosition) => GetStreamEventsByGlobalPositionInternal(this, streamId, minGlobalPosition);
		public IEnumerable<StreamEventDbModel> GetAllStreamEventsByGlobalPosition(long minGlobalPosition) => GetAllStreamEventsByGlobalPositionInternal(this, minGlobalPosition);
		public IEnumerable<StreamEventDbModel> GetSubscriptionEventsByGlobalPosition(string subscriptionName, long minGlobalPosition) => GetSubscriptionEventsByGlobalPositionInternal(this, subscriptionName, minGlobalPosition);

		// Note: Avoiding use of async queries due to this bug/performance issue that may no longer by an issue.
		// Tldr - async is an order of magnitude slower with tables that have varbinary(max), or was at the time of this discussion.
		// https://stackoverflow.com/questions/28543293/entity-framework-async-operation-takes-ten-times-as-long-to-complete

		private static Func<EventStoreDbContext, long?> GetMaxGlobalPositionInternal =
			EF.CompileQuery((EventStoreDbContext context) => context.StreamEvent.Max(x => (long?)x.GlobalPosition));

		private static Func<EventStoreDbContext, string, long?> GetLastStreamPositionByStreamIdInternal =
			EF.CompileQuery((EventStoreDbContext context, string streamId) =>
				context.StreamEvent
					.Where(x => x.StreamId == streamId) // No string transformation (i.e. ToUpper) assuming db is case insensitive by default.
					.Max(x => (long?)x.StreamPosition)
			);

		private static Func<EventStoreDbContext, string, long, IOrderedQueryable<StreamEventDbModel>> GetStreamEventsByStreamIdInternal =
			EF.CompileQuery((EventStoreDbContext context, string streamId, long minStreamPosition) =>
				context.StreamEvent
					.Where(x => x.StreamId == streamId && x.StreamPosition >= minStreamPosition) // No string transformation (i.e. ToUpper) assuming db is case insensitive by default.
					.OrderBy(x => x.StreamPosition)
			);

		private static Func<EventStoreDbContext, string, long, IEnumerable<StreamEventDbModel>> GetStreamEventsByGlobalPositionInternal =
			EF.CompileQuery((EventStoreDbContext context, string streamId, long minGlobalPosition) =>
				context.StreamEvent
					.Where(x => x.StreamId == streamId && x.GlobalPosition >= minGlobalPosition)
					.OrderBy(x => x.StreamPosition)
			);

		private static Func<EventStoreDbContext, long, IEnumerable<StreamEventDbModel>> GetAllStreamEventsByGlobalPositionInternal =
			EF.CompileQuery((EventStoreDbContext context, long minGlobalPosition) =>
				context.StreamEvent
					.Where(x => x.GlobalPosition >= minGlobalPosition)
					.OrderBy(x => x.StreamPosition)
			);

		private static Func<EventStoreDbContext, string, long, IEnumerable<StreamEventDbModel>> GetSubscriptionEventsByGlobalPositionInternal =
			EF.CompileQuery((EventStoreDbContext context, string subscriptionName, long minGlobalPosition) =>
				from se in context.StreamEvent
				from sub in context.SubscriptionFilter.Where(x => x.SubscriptionName == subscriptionName && se.StreamId.ToUpper().StartsWith(x.StreamIdPrefix.ToUpper()))
				orderby se.GlobalPosition
				select se
			);
	}
}
