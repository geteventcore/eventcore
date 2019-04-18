﻿using EventCore.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots.SerializableState
{
	public abstract class SerializableAggregateRootState<TInternalState> : AggregateRootState
	{
		private readonly ISerializableAggregateRootStateObjectRepo _repo;
		private readonly string _regionId;
		private readonly string _context;
		private readonly string _aggregateRootName;
		private readonly string _aggregateRootId;
		
		protected virtual int _maxCausalIdHistory { get; } = 1000;
		protected readonly List<string> _causalIdHistory = new List<string>();
		protected abstract TInternalState _internalState { get; set; }

		public SerializableAggregateRootState(
			IBusinessEventResolver resolver, ISerializableAggregateRootStateObjectRepo repo,
			string regionId, string context, string aggregateRootName, string aggregateRootId)
			: base(resolver)
		{
			_repo = repo;
			_regionId = regionId;
			_context = context;
			_aggregateRootName = aggregateRootName;
			_aggregateRootId = aggregateRootId;
		}

		public virtual async Task InitializeAsync(CancellationToken cancellationToken)
		{
			var stateObj = await _repo.LoadAsync(_regionId, _context, _aggregateRootName, _aggregateRootId, typeof(TInternalState));

			if (stateObj != null)
			{
				StreamPositionCheckpoint = stateObj.StreamPositionCheckpoint;
				_causalIdHistory.AddRange(stateObj.CausalIdHistory);

				if (stateObj.InternalState != null)
				{
					_internalState = (TInternalState)stateObj.InternalState;
				}
			}

		}

		public override async Task HydrateFromCheckpointAsync(Func<Func<StreamEvent, Task>, Task> streamLoaderAsync, CancellationToken cancellationToken)
		{
			await base.HydrateFromCheckpointAsync(streamLoaderAsync, cancellationToken);

			// Save state.
			var stateObj = new SerializableAggregateRootStateObject(StreamPositionCheckpoint, _causalIdHistory, _internalState);
			await _repo.SaveAsync(_regionId, _context, _aggregateRootName, _aggregateRootId, stateObj);
		}

		public override Task AddCausalIdToHistoryAsync(string causalId)
		{
			// Prevents large aggregate streams from accumulating too much history.
			// Causal id tracking is used for preventing duplicate commands, which
			// usually happens in quick succession.
			if (_causalIdHistory.Count >= _maxCausalIdHistory)
			{
				_causalIdHistory.Remove(_causalIdHistory.First());
			}
			_causalIdHistory.Add(causalId);

			return Task.CompletedTask;
		}

		public override Task<bool> IsCausalIdInHistoryAsync(string causalId)
		{
			// Causal id is case INsensitive.
			return Task.FromResult<bool>(_causalIdHistory.Exists(x => string.Equals(x, causalId, StringComparison.OrdinalIgnoreCase)));
		}
	}
}