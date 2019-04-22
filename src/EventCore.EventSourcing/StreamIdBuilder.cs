﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EventCore.EventSourcing
{
	public class StreamIdBuilder : IStreamIdBuilder
	{
		// Valid stream characters:
		// - ASCII letters and numbers
		// - underscore ("_")
		// - short dash / minus sign ("-")
		public const string INVALID_STREAM_ID_REGEX = @"[^A-Za-z0-9_-]";
		
		public const string SEPARATOR = "-";

		public static bool ValidateStreamIdChars(string chars) => !Regex.IsMatch(chars, INVALID_STREAM_ID_REGEX);

		public string Build(string regionId, string context, string aggregateRootName, string aggregateRootId)
		{
			if (string.IsNullOrWhiteSpace(aggregateRootName))
			{
				throw new ArgumentNullException("Aggregate root name is required.");
			}

			if (!ValidateStreamIdChars(regionId + context + aggregateRootName + aggregateRootId))
			{
				throw new ArgumentException("Invalid character(s) in stream id input.");
			}

			var composite = new List<string>();

			if (!string.IsNullOrWhiteSpace(regionId))
			{
				composite.Add(regionId);
			}

			if (!string.IsNullOrWhiteSpace(context))
			{
				composite.Add(context);
			}

			composite.Add(aggregateRootName); // Required.

			if (!string.IsNullOrWhiteSpace(aggregateRootId))
			{
				composite.Add(aggregateRootId);
			}

			return string.Join(SEPARATOR, composite);
		}
	}
}
