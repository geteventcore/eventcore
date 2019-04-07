using System;

namespace EventCore.StatefulSubscriber
{
	public class StreamState
	{
		public readonly long? LastProcessedPosition;
		public readonly bool HasError;

		public StreamState(long? lastProcessedPosition, bool hasError)
		{
			LastProcessedPosition = lastProcessedPosition;
			HasError = hasError;
		}
	}
}
