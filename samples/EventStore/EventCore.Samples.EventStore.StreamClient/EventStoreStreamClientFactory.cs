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

		// Event store connections are re-used, one per region, and disposed when the factory is disposed.
		// Factory is meant to be used as a singleton with dependency injection.
		private readonly Dictionary<string, IEventStoreConnection> _connections;

		public EventStoreStreamClientFactory(IStandardLogger logger, Dictionary<string, string> eventStoreUris, int streamReadBatchSize, int reconnectDelaySeconds)
		{
			_logger = logger;
			_eventStoreUris = eventStoreUris;
			_streamReadBatchSize = streamReadBatchSize;
			_reconnectDelaySeconds = reconnectDelaySeconds;

			_connections = new Dictionary<string, IEventStoreConnection>(eventStoreUris.Comparer);

			foreach (var regionId in _eventStoreUris.Keys)
			{
				// Create all connections so they're ready.
				var cnn = CreateConnection(regionId);
				cnn.ConnectAsync(); // Do not wait for connection to succeed or this will block service startup.
			}
		}

		private IEventStoreConnection CreateConnection(string regionId)
		{
			var uri = _eventStoreUris[regionId];
			var builder = ConnectionSettings.Create().KeepReconnecting(); // Very important. Attempt reconnect infinitely.
			var connection = EventStoreConnection.Create(uri, builder, $"Connection-{regionId}");

			connection.Connected += new EventHandler<ClientConnectionEventArgs>(delegate (Object o, ClientConnectionEventArgs a)
			{
				// _logger.LogDebug($"Event Store client connected. ({a.Connection.ConnectionName})");
			});

			connection.ErrorOccurred += new EventHandler<ClientErrorEventArgs>(delegate (Object o, ClientErrorEventArgs a)
			{
				_logger.LogError(a.Exception, $"Event store connection error. ({a.Connection.ConnectionName})");
			});

			_connections.Add(regionId, connection);
			return connection;
		}

		public IStreamClient Create(string regionId)
		{
			return new EventStoreStreamClient(_logger, _connections[regionId], new EventStoreStreamClientOptions(_streamReadBatchSize));
		}


		private bool disposed = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					foreach (var connection in _connections.Values)
					{
						connection.Dispose();
					}
				}
				disposed = true;
			}
		}

		public void Dispose() => Dispose(true);
	}
}
