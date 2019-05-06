using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventCore.EventSourcing;
using Newtonsoft.Json;

namespace EventCore.AggregateRoots.SerializableState
{
	public abstract class SerializableAggregateRootState<TInternalState> : AggregateRootState, ISerializableAggregateRootState
	{
		protected virtual int _maxCausalIdHistory { get; set; } = 100; // Limits the number of causal ids stored in memory/serialized.
		protected readonly List<string> _causalIdHistory = new List<string>(); // List<>, not Set<> - need ordering so we can remove overflow.
		protected abstract TInternalState _internalState { get; set; } // This is the concrete state's serializable object.

		public SerializableAggregateRootState(IBusinessEventResolver eventResolver) : base(eventResolver)
		{
		}

		public override Task AddCausalIdToHistoryAsync(string causalId)
		{
			// Prevents large aggregate streams from accumulating too much history.
			// Causal id tracking is used for preventing duplicate commands, which
			// usually happens in quick succession, so we don't need a long history.
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

		public virtual Task DeserializeInternalStateAsync(byte[] data)
		{
			var json = Encoding.Unicode.GetString(data);
			var deserializedState = (SerializableData<TInternalState>)JsonConvert.DeserializeObject(json, typeof(SerializableData<TInternalState>));

			if (deserializedState != null)
			{
				StreamPositionCheckpoint = deserializedState.StreamPositionCheckpoint;
				_causalIdHistory.AddRange(deserializedState.CausalIdHistory);

				if (deserializedState.InternalState != null)
				{
					_internalState = (TInternalState)deserializedState.InternalState;
				}
			}

			return Task.CompletedTask;
		}

		public virtual Task<byte[]> SerializeInternalStateAsync()
		{
			var serializableData = new SerializableData<TInternalState>(StreamPositionCheckpoint, _causalIdHistory, _internalState);
			var json = JsonConvert.SerializeObject(serializableData);
			return Task.FromResult(Encoding.Unicode.GetBytes(json));
		}
	}
}
