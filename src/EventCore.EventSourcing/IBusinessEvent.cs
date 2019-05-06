namespace EventCore.EventSourcing
{
	public interface IBusinessEvent
	{
		string GetCausalId();
		string GetCorrelationId();
	}
}
