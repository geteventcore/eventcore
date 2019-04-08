using System;

namespace EventCore.StatefulSubscriber
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
