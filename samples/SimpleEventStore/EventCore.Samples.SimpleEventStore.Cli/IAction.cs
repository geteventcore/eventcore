using System.Threading.Tasks;

namespace EventCore.Samples.SimpleEventStore.Cli
{
	public interface IAction
	{
		Task RunAsync();
	}
}
