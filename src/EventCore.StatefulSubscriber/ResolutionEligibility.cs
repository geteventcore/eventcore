namespace EventCore.StatefulSubscriber
{
	public enum ResolutionEligibility
	{
		Eligible, UnableStreamHasError, UnableToResolveEventType, SkippedAlreadyProcessed
	}
}
