﻿using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain
{
	public interface IApplyBusinessEvent<TEvent> where TEvent : BusinessEvent
	{
		Task ApplyBusinessEventAsync(string streamId, long position, TEvent e, CancellationToken cancellationToken);
	}
}
