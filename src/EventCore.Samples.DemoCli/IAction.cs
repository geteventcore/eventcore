using System.Threading.Tasks;

namespace EventCore.Samples.DemoCli
{
	public interface IAction
	{
		Task RunAsync();
	}
}
