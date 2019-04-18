using System;

namespace EventCore.EventSourcing
{
	public interface IBusinessEventResolver
	{
		bool CanResolve(string type);
		bool CanUnresolve(BusinessEvent e);
		BusinessEvent Resolve(string type, byte[] data);
		UnresolvedBusinessEvent Unresolve(BusinessEvent e);
	}
}
