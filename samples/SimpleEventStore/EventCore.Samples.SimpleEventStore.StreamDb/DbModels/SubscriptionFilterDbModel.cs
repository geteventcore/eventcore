﻿namespace EventCore.Samples.SimpleEventStore.StreamDb.DbModels
{
	public class SubscriptionFilterDbModel
	{
		public string SubscriptionName { get; set; }
		public string StreamIdPrefix { get; set; }
	}
}