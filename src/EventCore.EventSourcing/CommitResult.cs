using System;

namespace EventCore.EventSourcing
{
	public enum CommitResult
	{
		Success, ConcurrencyConflict
	}
}
