namespace EventCore.EventSourcing
{
	public interface IStreamIdBuilder
	{
		string Build(string regionId, string context, string entityName, string entityId);
	}
}
