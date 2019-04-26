﻿using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain
{
	public abstract class DomainCommand : Command
	{
		// Use of underscore breaks naming conventions for pubilc fields but since commands are
		// simple data transfer objects we want to clearly separate which properties/methods are
		// for the system and which are for the data represented in subclasses.
		
		public string _CommandName
		{
			get
			{
				var commandName = this.GetType().Name;
				var removeString = "Command";
				int lastIndex = commandName.LastIndexOf(removeString, StringComparison.OrdinalIgnoreCase);
				return (lastIndex < 0)
						? commandName
						: commandName.Remove(lastIndex, removeString.Length);
			}
		}

		public abstract string _AggregateRootName { get; }

		public override string GetRegionId() => Constants.DEFAULT_REGION_ID; // Only one region for now.

		public DomainCommand(ICommandMetadata _metadata) : base(_metadata)
		{
		}
	}
}