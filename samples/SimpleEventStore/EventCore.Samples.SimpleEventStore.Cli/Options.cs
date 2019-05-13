using CommandLine;

namespace EventCore.Samples.SimpleEventStore.Cli
{
	public static class Options
	{
		[Verb("subscribe", HelpText = "Subscribe to all new events.")]
		public class SubscribeOptions
		{
		}
	}
}
