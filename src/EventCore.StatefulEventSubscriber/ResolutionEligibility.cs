namespace EventCore.StatefulEventSubscriber
{
	public enum ResolutionEligibility
	{
		Eligible, UnableStreamHasError,UnableToResolveEventType, SkippedAlreadyProcessed
	}
}
