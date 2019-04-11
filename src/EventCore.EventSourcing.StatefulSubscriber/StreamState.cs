using System;

namespace EventCore.EventSourcing.StatefulSubscriber
{
	public class StreamState
	{
		public readonly long LastAttemptedPosition;
		public readonly bool HasError;

		public StreamState(long lastAttemptedPosition, bool hasError)
		{
			LastAttemptedPosition = lastAttemptedPosition;
			HasError = hasError;
		}
	}
}
