using System;

namespace EventCore.EventSourcing
{
	public interface IBusinessEventResolver
	{
		bool CanResolve(string type);
		bool CanUnresolve(BusinessEvent e);
		BusinessEvent ResolveEvent(string type, byte[] data);
		UnresolvedBusinessEvent UnresolveEvent(BusinessEvent e);
	}
}
