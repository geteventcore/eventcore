using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface IHandlingManagerTaskCollection
	{
		void Add(string key, Task task);
		IList<string> Keys { get; }
		void PurgeFinishedTasks();
	}
}
