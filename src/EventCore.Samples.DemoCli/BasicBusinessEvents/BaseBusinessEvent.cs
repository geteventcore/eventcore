using CommandLine;
using EventCore.EventSourcing;

namespace EventCore.Samples.DemoCli.BasicBusinessEvents
{
	public class BaseBusinessEvent : BusinessEvent
	{
		public string Message {get;}

		public BaseBusinessEvent(string causalId, string correlationId, string message) : base(causalId, correlationId)
		{
			Message = message;
		}
	}

	public class BusinessEventT01 : BaseBusinessEvent
	{
		public BusinessEventT01(string causalId, string correlationId, string message) : base(causalId, correlationId, message)
		{
		}
	}

	public class BusinessEventT02 : BaseBusinessEvent
	{
		public BusinessEventT02(string causalId, string correlationId, string message) : base(causalId, correlationId, message)
		{
		}
	}

	public class BusinessEventT03 : BaseBusinessEvent
	{
		public BusinessEventT03(string causalId, string correlationId, string message) : base(causalId, correlationId, message)
		{
		}
	}

	public class BusinessEventT04 : BaseBusinessEvent
	{
		public BusinessEventT04(string causalId, string correlationId, string message) : base(causalId, correlationId, message)
		{
		}
	}

	public class BusinessEventT05 : BaseBusinessEvent
	{
		public BusinessEventT05(string causalId, string correlationId, string message) : base(causalId, correlationId, message)
		{
		}
	}

	public class BusinessEventT06 : BaseBusinessEvent
	{
		public BusinessEventT06(string causalId, string correlationId, string message) : base(causalId, correlationId, message)
		{
		}
	}

	public class BusinessEventT07 : BaseBusinessEvent
	{
		public BusinessEventT07(string causalId, string correlationId, string message) : base(causalId, correlationId, message)
		{
		}
	}

	public class BusinessEventT08 : BaseBusinessEvent
	{
		public BusinessEventT08(string causalId, string correlationId, string message) : base(causalId, correlationId, message)
		{
		}
	}

	public class BusinessEventT09 : BaseBusinessEvent
	{
		public BusinessEventT09(string causalId, string correlationId, string message) : base(causalId, correlationId, message)
		{
		}
	}

	public class BusinessEventT10 : BaseBusinessEvent
	{
		public BusinessEventT10(string causalId, string correlationId, string message) : base(causalId, correlationId, message)
		{
		}
	}
}
