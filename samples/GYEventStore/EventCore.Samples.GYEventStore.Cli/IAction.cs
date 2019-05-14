using System.Threading.Tasks;

namespace EventCore.Samples.GYEventStore.Cli
{
	public interface IAction
	{
		Task RunAsync();
	}
}
