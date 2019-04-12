using EventCore.AggregateRoots;
using EventCore.Samples.EmailSystem.Domain.EmailQueue.StateModels;
using System;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue
{
    public class EmailQueueState : GenericAggregateRootState
    {
			public EmailQueueMessage Message {get;}
    }
}
