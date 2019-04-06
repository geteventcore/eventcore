﻿using System;

namespace EventCore.Utilities
{
	public interface IGenericLogger
	{
		void LogCritical(string message, params object[] args);
		void LogCritical(Exception ex, string message, params object[] args);
		void LogError(string message, params object[] args);
		void LogError(Exception ex, string message, params object[] args);
		void LogInformation(string message, params object[] args);
		void LogTrace(string message, params object[] args);
		void LogWarning(string message, params object[] args);
		void LogWarning(Exception ex, string message, params object[] args);
	}

	public interface IGenericLogger<T> : IGenericLogger { }
}
