using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Cli
{
	public interface IAction
	{
		Task RunAsync();
	}
}
