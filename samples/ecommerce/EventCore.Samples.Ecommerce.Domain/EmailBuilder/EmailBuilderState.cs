﻿using EventCore.AggregateRoots;
using EventCore.AggregateRoots.DatabaseState;
using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.StateModels;
using EventCore.Samples.Ecommerce.Domain.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder
{
	public partial class EmailBuilderState : AggregateRootState, IDatabaseAggregateRootState
	{
		private readonly EmailBuilderDbContext _db;

		public EmailBuilderState(IBusinessEventResolver eventResolver, EmailBuilderDbContext db) : base(eventResolver)
		{
			_db = db;
		}

		protected override Task AddCausalIdToHistoryAsync(string causalId) => _db.AddCausalIdToHistoryIfNotExistsAsync(causalId);
		public override Task<bool> IsCausalIdInHistoryAsync(string causalId) => _db.ExistsCausalIdInHistoryAsync(causalId);
		public Task SaveChangesAsync(CancellationToken cancellationToken) => _db.SaveChangesAsync();
	}
}