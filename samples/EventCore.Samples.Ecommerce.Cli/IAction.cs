using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Cli
{
	public interface IAction
	{
		Task RunAsync();
	}
}
