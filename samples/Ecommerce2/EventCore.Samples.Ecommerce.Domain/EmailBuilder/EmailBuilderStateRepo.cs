using EventCore.AggregateRoots.DatabaseState;
using EventCore.EventSourcing;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder
{
	public class EmailBuilderStateRepo : DatabaseAggregateRootStateRepo<EmailBuilderState>
	{
		private const int SAVE_BATCH_SIZE = 30; // How many events to apply before saving to database.

		// private static Func<string, EmailBuilderState> = (regionId) => new Email

		public EmailBuilderStateRepo(IStreamClientFactory streamClientFactory, EmailBuilderStateFactory stateFactory)
			: base(streamClientFactory, (regionId) => stateFactory.Create(regionId), SAVE_BATCH_SIZE)
		{
		}
	}
}