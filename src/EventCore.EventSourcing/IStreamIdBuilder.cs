namespace EventCore.EventSourcing
{
	public interface IStreamIdBuilder
	{
		string Build(string regionId, string context, string aggregateRootName, string aggregateRootId);
	}
}
