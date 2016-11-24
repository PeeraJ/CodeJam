﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using BenchmarkDotNet.Helpers;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>Core logic for source annotations.</summary>
	internal static partial class SourceAnnotationsHelper
	{
		#region Helper types
		/// <summary>
		/// Helper class that stores files' content during the annotation
		/// </summary>
		private class AnnotateContext
		{
			private readonly Dictionary<string, string[]> _sourceLines = new Dictionary<string, string[]>();
			private readonly Dictionary<string, XDocument> _xmlAnnotations = new Dictionary<string, XDocument>();
			private readonly HashSet<string> _changedFiles = new HashSet<string>();

			private bool HasChanges => _changedFiles.Any();

			/// <summary>Tries to get the lines of the source file.</summary>
			/// <param name="file">The sources file.</param>
			/// <param name="competitionState">State of the run.</param>
			/// <returns>Lines of the source file or empty collection if none.</returns>
			[NotNull]
			public IReadOnlyList<string> TryGetFileLines(
				[NotNull] string file,
				[NotNull] CompetitionState competitionState)
			{
				Code.NotNullNorEmpty(file, nameof(file));
				Code.NotNull(competitionState, nameof(competitionState));
				Code.AssertState(!_xmlAnnotations.ContainsKey(file), $"File '{file}' already loaded as XML annotation.");

				return _sourceLines.GetOrAdd(
					file, f =>
					{
						try
						{
							return File.ReadAllLines(f);
						}
						catch (IOException ex)
						{
							competitionState.WriteExceptionMessage(
								MessageSource.Analyser, MessageSeverity.SetupError,
								$"Could not access file '{file}'.", ex);

							return Array<string>.Empty;
						}
						catch (UnauthorizedAccessException ex)
						{
							competitionState.WriteExceptionMessage(
								MessageSource.Analyser, MessageSeverity.SetupError,
								$"Could not access file '{file}'.", ex);

							return Array<string>.Empty;
						}
					});
			}

			/// <summary>Tries lo load the xml annotation document.</summary>
			/// <param name="file">The file with xml annotation.</param>
			/// <param name="useFullTypeName">Use full type name in XML annotations.</param>
			/// <param name="competitionState">State of the run.</param>
			/// <returns>The document with competition limits for the benchmark or <c>null</c> if none.</returns>
			public XDocument TryGetXmlAnnotation(
				[NotNull] string file,
				bool useFullTypeName,
				[NotNull] CompetitionState competitionState)
			{
				Code.NotNullNorEmpty(file, nameof(file));
				Code.NotNull(competitionState, nameof(competitionState));
				Code.AssertState(!_sourceLines.ContainsKey(file), $"File '{file}' already loaded as source lines.");

				return _xmlAnnotations.GetOrAdd(
					file, f =>
					{
						try
						{
							using (var reader = File.OpenText(f))
							{
								return XmlAnnotations.TryParseXmlAnnotationDoc(
									reader, useFullTypeName, competitionState,
									$"XML annotation '{file}'");
							}
						}
						catch (IOException ex)
						{
							competitionState.WriteExceptionMessage(
								MessageSource.Analyser, MessageSeverity.SetupError,
								$"Could not access file '{file}'.", ex);

							return null;
						}
						catch (UnauthorizedAccessException ex)
						{
							competitionState.WriteExceptionMessage(
								MessageSource.Analyser, MessageSeverity.SetupError,
								$"Could not access file '{file}'.", ex);

							return null;
						}
					});
			}

			/// <summary>Marks file as changed. File should be loaded before calling this method.</summary>
			/// <param name="file">The file to mark.</param>
			public void MarkAsChanged(string file)
			{
				if (_sourceLines.GetValueOrDefault(file).IsNullOrEmpty() &&
					_xmlAnnotations.GetValueOrDefault(file) == null)
					throw CodeExceptions.InvalidOperation($"Load file '{file}' before marking it as changed.");

				_changedFiles.Add(file);
			}

			/// <summary>
			/// Replaces source line and marks file as changed line. Sources file should be loaded before calling this method.
			/// </summary>
			/// <param name="file">The file with sources.</param>
			/// <param name="lineIndex">Index of the line to replace.</param>
			/// <param name="newLine">Line to replace with.</param>
			public void ReplaceLine(string file, int lineIndex, string newLine)
			{
				var lines = _sourceLines.GetValueOrDefault(file);
				if (lines.IsNullOrEmpty())
					throw CodeExceptions.InvalidOperation($"Load source file '{file}' before marking it as changed.");

				lines[lineIndex] = newLine;
				MarkAsChanged(file);
			}

			/// <summary>Saves modified files.</summary>
			public void Save()
			{
				if (!HasChanges)
					return;

				foreach (var pair in _sourceLines)
				{
					if (_changedFiles.Contains(pair.Key))
					{
						BenchmarkHelpers.WriteFileContent(pair.Key, pair.Value);
					}
				}

				foreach (var pair in _xmlAnnotations)
				{
					if (_changedFiles.Contains(pair.Key))
					{
						using (var writer = new StreamWriter(pair.Key))
						{
							XmlAnnotations.Save(pair.Value, writer);
						}
					}
				}
			}
		}
		#endregion

		/// <summary>Tries to annotate source files with competition limits.</summary>
		/// <param name="targetsToAnnotate">Benchmarks to annotate.</param>
		/// <param name="competitionState">State of the run.</param>
		/// <returns>Array of successfully annotated benchmarks.</returns>
		[NotNull]
		public static CompetitionTarget[] TryAnnotateBenchmarkFiles(
			[NotNull] CompetitionTarget[] targetsToAnnotate,
			[NotNull] CompetitionState competitionState)
		{
			Code.NotNull(targetsToAnnotate, nameof(targetsToAnnotate));
			Code.NotNull(competitionState, nameof(competitionState));

			var annotatedTargets = new List<CompetitionTarget>();
			var annContext = new AnnotateContext();

			foreach (var targetToAnnotate in targetsToAnnotate)
			{
				var target = targetToAnnotate.Target;
				var targetMethodTitle = target.MethodDisplayInfo;

				competitionState.WriteVerbose(
					$"Method {targetMethodTitle}: updating time limits {targetToAnnotate}.");

				// DONTTOUCH: the source should be loaded for checksum validation even if target uses resource annotation.
				string fileName;
				int firstCodeLine;
				var hasSource = SymbolHelpers.TryGetSourceInfo(target.Method, competitionState, out fileName, out firstCodeLine);

				if (!hasSource)
				{
					continue;
				}

				var competitionMetadata = targetToAnnotate.CompetitionMetadata;
				if (competitionMetadata != null)
				{
					var resourceFileName = GetResourceFileName(fileName, competitionMetadata);

					competitionState.WriteVerbose(
						$"Method {targetMethodTitle}: annotating resource file '{resourceFileName}'.");
					var annotated = TryFixBenchmarkXmlAnnotation(annContext, resourceFileName, targetToAnnotate, competitionState);
					if (!annotated)
					{
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.Warning,
							$"Method {targetMethodTitle}: could not annotate resource file '{resourceFileName}'.");
						continue;
					}
				}
				else
				{
					competitionState.WriteVerbose(
						$"Method {targetMethodTitle}: annotating file '{fileName}', line {firstCodeLine}.");
					var annotated = TryFixBenchmarkAttribute(annContext, fileName, firstCodeLine, targetToAnnotate, competitionState);
					if (!annotated)
					{
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.Warning,
							$"Method {targetMethodTitle}: could not annotate source file '{fileName}'.");
						continue;
					}
				}

				competitionState.WriteVerboseDiagnostic(
					$"Method {targetMethodTitle}: updated with time limits {targetToAnnotate}.");
				annotatedTargets.Add(targetToAnnotate);
			}

			annContext.Save();
			return annotatedTargets.ToArray();
		}
	}
}