﻿using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public interface IHandleBusinessEvent<TEvent> where TEvent : IBusinessEvent
	{
		Task HandleBusinessEventAsync(string streamId, long position, TEvent e, CancellationToken cancellationToken);
	}
}
