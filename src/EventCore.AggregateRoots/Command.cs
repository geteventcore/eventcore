namespace EventCore.AggregateRoots
{
	public abstract class Command
	{
		public readonly CommandMetadata Metadata;

		public Command(CommandMetadata metadata)
		{
			Metadata = metadata;
		}

		public abstract string AggregateRootId();
	}
}
