using System;

namespace EventCore.EventSourcing
{
	public interface IBusinessEventResolver
	{
		bool CanResolve(string type);
		bool CanUnresolve(IBusinessEvent e);
		IBusinessEvent Resolve(string type, byte[] data);
		UnresolvedBusinessEvent Unresolve(IBusinessEvent e);
	}
}
