using EventCore.EventSourcing;
using EventCore.Utilities;
using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EventStore.StreamClient
{
	public class EventStoreStreamClientFactory : IStreamClientFactory, IDisposable
	{
		private readonly IStandardLogger _logger;
		private readonly Dictionary<string, string> _eventStoreUris;
		private readonly int _streamReadBatchSize;
		private readonly int _reconnectDelaySeconds;

		private readonly Dictionary<string, EventStoreStreamClient> _streamClients;

		public EventStoreStreamClientFactory(IStandardLogger logger, Dictionary<string, string> eventStoreUris, int streamReadBatchSize, int reconnectDelaySeconds)
		{
			_logger = logger;
			_eventStoreUris = eventStoreUris;
			_streamReadBatchSize = streamReadBatchSize;
			_reconnectDelaySeconds = reconnectDelaySeconds;

			_streamClients = new Dictionary<string, EventStoreStreamClient>(eventStoreUris.Comparer);

			foreach (var regionId in _eventStoreUris.Keys)
			{
				var connection = CreateConnection(_eventStoreUris[regionId], regionId);
				var streamClient = new EventStoreStreamClient(_logger, connection, new EventStoreStreamClientOptions(_streamReadBatchSize));
				_streamClients.Add(regionId, streamClient);

				connection.ConnectAsync().Wait(); // Event Store connections are designed to be long-lived and thread-safe.
			}
		}

		private IEventStoreConnection CreateConnection(string uri, string regionId)
		{
			var cnn = EventStoreConnection.Create(uri, $"Connection-{regionId}");
			cnn.Closed += new EventHandler<ClientClosedEventArgs>(delegate (Object o, ClientClosedEventArgs a)
			{
				_logger.LogWarning($"Event Store connection closed. Reconnecting after {_reconnectDelaySeconds} seconds. ({cnn.ConnectionName})");
				Thread.Sleep(_reconnectDelaySeconds * 1000);
				a.Connection.ConnectAsync().Wait();
			});
			return cnn;
		}

		public IStreamClient Create(string regionId)
		{
			return _streamClients[regionId];
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (var connection in _streamClients.Values)
					{
						connection.Dispose();
					}
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~EventStoreStreamClientFactory()
		// {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
