using Microsoft.Extensions.Logging.Abstractions;

namespace EventCore.Utilities
{
	public class NullStandardLogger : StandardLogger
	{
		public static NullStandardLogger Instance { get => new NullStandardLogger(); }
		
		public NullStandardLogger() : base(NullLogger.Instance)
		{
		}
	}
}
