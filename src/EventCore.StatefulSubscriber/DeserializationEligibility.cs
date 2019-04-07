namespace EventCore.StatefulSubscriber
{
	public enum DeserializationEligibility
	{
		Eligible, UnableStreamHasError,UnableToResolveEventType, SkippedAlreadyProcessed
	}
}
