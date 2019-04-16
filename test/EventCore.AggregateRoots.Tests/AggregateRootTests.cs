using System;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class AggregateRootTests
	{
		[Fact]
		public void handle_command_should_return_semantic_validation_errors()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void handle_command_should_return_duplicate_command_id_validation_errors()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void handle_command_should_create_state_from_serialized_data()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void handle_command_should_return_state_validation_errors()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void handle_command_should_process_command_to_committed_events_and_return_success()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void handle_command_should_save_serialized_state()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void try_load_serialized_state_should_return_null_when_not_support_serialization()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void try_load_serialized_state_should_return_null_when_exception()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void try_load_serialized_state_should_return_loaded_state()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void try_save_serialized_state_should_do_nothing_null_when_agg_root_not_support_serialization()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void try_save_serialized_state_should_do_nothing_null_when_state_not_support_serialization()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void try_save_serialized_state_should_do_nothing_when_exception()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void try_save_serialized_state_should_save_given_state()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void process_events_result_should_do_nothing_with_empty_events()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void process_events_result_should_throw_when_can_not_unresolve_event()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void process_events_result_should_commit_events()
		{
			throw new NotImplementedException();
		}
	}
}
