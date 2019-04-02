using Microsoft.Extensions.Logging.Abstractions;

namespace EventCore.Utilities
{
	public class NullGenericLogger : GenericLogger
	{
		public static NullGenericLogger Instance { get => new NullGenericLogger(); }
		
		public NullGenericLogger() : base(NullLogger.Instance)
		{
		}
	}
}
