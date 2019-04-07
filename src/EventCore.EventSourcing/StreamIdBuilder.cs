using System;
using System.Text.RegularExpressions;

namespace EventCore.EventSourcing
{
	public class StreamIdBuilder
	{
		private const string INVALID_STREAM_ID_REGEX = @"[^A-Za-z0-9._-]";

		public readonly string StreamId;

		public StreamIdBuilder(string aggregateName, string aggregateId)
		{
			if (!ValidateStreamIdFragment(aggregateName + aggregateId))
			{
				throw new ArgumentException("Invalid character(s) in stream id input.");
			}

			StreamId = aggregateName + "-" + aggregateId;
		}

		public static bool ValidateStreamIdFragment(string fragment) => !Regex.IsMatch(fragment, INVALID_STREAM_ID_REGEX);
	}
}
