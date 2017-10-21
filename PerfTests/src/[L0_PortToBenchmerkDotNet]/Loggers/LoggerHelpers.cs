﻿using System;
using System.Linq;
using System.Text;

using BenchmarkDotNet.Configs;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Loggers
{
	/// <summary>
	/// Logger helpers.
	/// </summary>
	public static class LoggerHelpers
	{
		private const int SeparatorLength = 42;

		/// <summary>Helper method that writes separator log line.</summary>
		/// <param name="logger">The logger.</param>
		public static void WriteSeparatorLine([NotNull] this ILogger logger) =>
			WriteSeparatorLine(logger, null, false);

		/// <summary>Helper method that writes separator log line.</summary>
		/// <param name="logger">The logger.</param>
		/// <param name="header">The separator line text.</param>
		public static void WriteSeparatorLine([NotNull] this ILogger logger, [CanBeNull] string header) =>
			WriteSeparatorLine(logger, header, false);

		/// <summary>Helper method that writes separator log line.</summary>
		/// <param name="logger">The logger.</param>
		/// <param name="header">The separator line text.</param>
		/// <param name="topHeader">Write top-level header.</param>
		public static void WriteSeparatorLine(
			[NotNull] this ILogger logger, [CanBeNull] string header, bool topHeader)
		{
			var separatorChar = topHeader ? '=' : '-';
			var logKind = topHeader ? LogKind.Header : LogKind.Help;

			var result = new StringBuilder(SeparatorLength);

			if (!string.IsNullOrEmpty(header))
			{
				var prefixLength = (SeparatorLength - header.Length - 2) / 2;
				if (prefixLength > 0)
				{
					result.Append(separatorChar, prefixLength);
				}
				result.Append(' ').Append(header).Append(' ');
			}

			var suffixLength = SeparatorLength - result.Length;
			if (suffixLength > 0)
			{
				result.Append(separatorChar, suffixLength);
			}

			logger.WriteLine();
			logger.WriteLine(logKind, result.ToString());
		}

		/// <summary>Flushes the loggers.</summary>
		/// <param name="config">Config with loggers.</param>
		public static void FlushLoggers(IConfig config)
		{
			var loggers = config.GetLoggers().OfType<IFlushableLogger>();
			foreach (var flushable in loggers)
			{
				flushable.Flush();
			}
		}
	}
}