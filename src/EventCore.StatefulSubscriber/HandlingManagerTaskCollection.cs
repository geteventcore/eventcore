﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public class HandlingManagerTaskCollection : IHandlingManagerTaskCollection
	{
		private readonly Dictionary<string, Task> _tasks = new Dictionary<string, Task>(StringComparer.OrdinalIgnoreCase);

		public IList<string> Keys => _tasks.Keys.ToList();

		public HandlingManagerTaskCollection()
		{
		}

		public void Add(string key, Task task) => _tasks.Add(key, task);

		public void PurgeFinishedTasks()
		{
			foreach (var key in _tasks.Where(kvp => kvp.Value.IsCanceled || kvp.Value.IsCompleted || kvp.Value.IsFaulted).Select(kvp => kvp.Key).ToList())
			{
				_tasks.Remove(key);
			}
		}
	}
}
