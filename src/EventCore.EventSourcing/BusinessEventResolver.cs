using EventCore.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventCore.EventSourcing
{
	// Generic resolver that uses business event class names as event type.
	// Type names are case insensitive.
	public class BusinessEventResolver : IBusinessEventResolver
	{
		private readonly IStandardLogger _logger;

		private readonly Dictionary<string, Type> _typeNamesToStrongTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<Type, string> _strongTypesToTypeNames = new Dictionary<Type, string>();

		public bool CanResolve(string type) => _typeNamesToStrongTypes.ContainsKey(type);
		public bool CanUnresolve(IBusinessEvent e) => _strongTypesToTypeNames.ContainsKey(e.GetType());

		public BusinessEventResolver(IStandardLogger logger, ISet<Type> businessEventTypes)
		{
			_logger = logger;

			foreach (var type in businessEventTypes)
			{
				if (!type.GetInterfaces().Contains(typeof(IBusinessEvent)))
					throw new ArgumentException("Type must be of IBusinessEvent.");

				_typeNamesToStrongTypes.Add(type.Name, type);
				_strongTypesToTypeNames.Add(type, type.Name);
			}
		}

		public IBusinessEvent Resolve(string type, byte[] data)
		{
			var strongType = _typeNamesToStrongTypes[type]; // Allow to throw unhandled exception.

			try
			{
				var json = Encoding.Unicode.GetString(data);
				return (IBusinessEvent)JsonConvert.DeserializeObject(json, strongType);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while resolving business event.");
				return null; // Intentionally resolve to null when serialization or encoding exception.
			}
		}

		public UnresolvedBusinessEvent Unresolve(IBusinessEvent e)
		{
			var typeName = _strongTypesToTypeNames[e.GetType()]; // Allow to throw unhandled exception.

			try
			{
				var json = JsonConvert.SerializeObject(e);
				var data = Encoding.Unicode.GetBytes(json);

				return new UnresolvedBusinessEvent(typeName, data);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while unresolving business event.");
				return null; // Intentionally resolve to null when serialization or encoding exception.
			}
		}
	}
}
