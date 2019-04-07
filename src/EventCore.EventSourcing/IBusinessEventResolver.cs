using System;

namespace EventCore.EventSourcing
{
	public interface IBusinessEventResolver
	{
		bool CanResolve(string type);
		BusinessEvent ResolveEvent(string type, byte[] data);
	}
}
