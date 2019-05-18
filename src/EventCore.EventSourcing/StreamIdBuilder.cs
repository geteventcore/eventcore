using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EventCore.EventSourcing
{
	public class StreamIdBuilder : IStreamIdBuilder
	{
		public const string SEPARATOR = "-";

		public string Build(string regionId, string context, string entityName, string entityId)
		{
			if (string.IsNullOrWhiteSpace(entityName))
			{
				throw new ArgumentNullException("Entity name is required.");
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

			composite.Add(entityName); // Required.

			if (!string.IsNullOrWhiteSpace(entityId))
			{
				composite.Add(entityId);
			}

			return TreatStreamId(string.Join(SEPARATOR, composite));
		}

		// Upper case all stream ids, event store implementations are not guaranteed to be case insensitive.
		public virtual string TreatStreamId(string untreatedStreamId) => untreatedStreamId.ToUpper();
	}
}
