namespace EventCore.EventSourcing
{
	public interface IStreamClientFactory
	{
		IStreamClient Create(string regionId);
	}
}
