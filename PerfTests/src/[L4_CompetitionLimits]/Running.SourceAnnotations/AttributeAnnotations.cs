﻿using System;
using System.Reflection;

using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Helper class for attribute annotations
	/// </summary>
	internal static class AttributeAnnotations
	{
		/// <summary>
		/// Returns the name of target resource if defined in <see cref="CompetitionMetadataAttribute"/>.
		/// If the target type is nested all container types are checked too.
		/// </summary>
		/// <param name="target">The target to get resource name for.</param>
		/// <returns>
		/// Name of the resource containing xml document with competition limits
		/// or <c>null</c> if the target is not annotated with <see cref="CompetitionMetadataAttribute"/>
		/// </returns>
		[CanBeNull]
		public static CompetitionMetadata TryGetCompetitionMetadata([NotNull] this Target target)
		{
			Code.NotNull(target, nameof(target));

			CompetitionMetadataAttribute result = null;

			var targetType = target.Type;
			while (result == null && targetType != null)
			{
				result = targetType.GetCustomAttribute<CompetitionMetadataAttribute>();

				targetType = targetType.DeclaringType;
			}

			if (result == null)
				return null;

			return new CompetitionMetadata(
				result.MetadataResourceName,
				result.MetadataResourcePath,
				result.UseFullTypeName);
		}

		/// <summary>
		/// Creates <see cref="CompetitionLimit"/> from <see cref="CompetitionBenchmarkAttribute"/>.
		/// </summary>
		/// <param name="competitionAttribute">The attribute with competition limits.</param>
		/// <returns>
		/// A new instance of the <see cref="CompetitionLimit"/> class
		/// filled with the properties from <see cref="CompetitionBenchmarkAttribute"/>
		/// </returns>
		public static CompetitionLimit ParseCompetitionLimit(
			[NotNull] CompetitionBenchmarkAttribute competitionAttribute)
		{
			Code.NotNull(competitionAttribute, nameof(competitionAttribute));

			return new CompetitionLimit(competitionAttribute.MinRatio, competitionAttribute.MaxRatio);
		}
	}
}