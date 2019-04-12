namespace EventCore.AggregateRoots
{
	public interface IAggregateRootStateFactory<TState> where TState : IAggregateRootState
	{
		TState Create(string serializedState = null);
	}
}
