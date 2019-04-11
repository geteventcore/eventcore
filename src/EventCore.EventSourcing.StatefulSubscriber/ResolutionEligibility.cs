namespace EventCore.EventSourcing.StatefulSubscriber
{
	public enum ResolutionEligibility
	{
		Eligible, UnableStreamHasError,UnableToResolveEventType, SkippedAlreadyProcessed
	}
}
