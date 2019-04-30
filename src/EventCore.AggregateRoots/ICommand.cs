namespace EventCore.AggregateRoots
{
	public interface ICommand
	{	
		// Methods should be prefixed with a verb to reduce the chance of naming collision/confusion
		// with data in subclasses.
		string GetCommandId();
		string GetRegionId();
		string GetAggregateRootId();
		ICommandValidationResult ValidateSemantics();
	}
}
