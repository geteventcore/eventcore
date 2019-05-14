using CommandLine;

namespace EventCore.Samples.GYEventStore.Cli
{
	public static class Options
	{
		[Verb("subscribe", HelpText = "Subscribe to all new events.")]
		public class SubscribeOptions
		{
		}
	}
}
