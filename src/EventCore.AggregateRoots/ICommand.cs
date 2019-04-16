namespace EventCore.AggregateRoots
{
	public interface ICommand
	{
		// Use of underscore breaks naming conventions but since commands are
		// simple data transfer objects we want to make it clear which properties/methods are
		// for the system and which are for the data represented in subclasses.
		ICommandMetadata _Metadata { get; }
		
		// Methods should be prefixed with a verb to reduce the chance of naming collision/confusion
		// with data in subclasses.
		string GetRegionId();
		string GetAggregateRootId();
		ICommandValidationResult ValidateSemantics();
	}
}
