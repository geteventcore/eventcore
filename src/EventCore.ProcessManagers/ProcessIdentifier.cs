namespace EventCore.ProcessManagers
{
	public struct ProcessIdentifier
	{
		public string ProcessType, CorrelationId;

		public ProcessIdentifier(string processType, string correlationId)
		{
			ProcessType = processType;
			CorrelationId = correlationId;
		}
	}
}
