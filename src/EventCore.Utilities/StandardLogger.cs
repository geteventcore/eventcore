﻿using Microsoft.Extensions.Logging;
using System;

namespace EventCore.Utilities
{
	public class StandardLogger<T> : StandardLogger, IStandardLogger<T>
	{
		public StandardLogger(ILogger<T> logger) : base(logger) { }
	}

	public class StandardLogger : IStandardLogger
	{
		protected readonly ILogger _logger;

		public StandardLogger(ILogger logger)
		{
			_logger = logger;
		}

		public void LogDebug(string message, params object[] args) => _logger.LogDebug(message, args);
		public void LogDebug(Exception ex, string message, params object[] args) => _logger.LogDebug(ex, message, args);
		public void LogCritical(string message, params object[] args) => _logger.LogCritical(message, args);
		public void LogCritical(Exception ex, string message, params object[] args) => _logger.LogCritical(ex, message, args);
		public void LogError(string message, params object[] args) => _logger.LogError(message, args);
		public void LogError(Exception ex, string message, params object[] args) => _logger.LogError(ex, message, args);
		public void LogInformation(string message, params object[] args) => _logger.LogInformation(message, args);
		public void LogTrace(string message, params object[] args) => _logger.LogTrace(message, args);
		public void LogWarning(string message, params object[] args) => _logger.LogWarning(message, args);
		public void LogWarning(Exception ex, string message, params object[] args) => _logger.LogWarning(ex, message, args);
	}
}
