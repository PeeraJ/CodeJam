﻿using System;

using BenchmarkDotNet.Attributes;

using JetBrains.Annotations;

namespace BenchmarkDotNet.NUnit
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	[PublicAPI]
	public class CompetitionBenchmarkAttribute : BenchmarkAttribute
	{
		public CompetitionBenchmarkAttribute()
		{}

		public CompetitionBenchmarkAttribute(double minRatio, double maxRatio)
		{
			MinRatio = minRatio;
			MaxRatio = maxRatio;
		}

		public bool DoesNotCompete { get; set; }

		public double MaxRatio { get; private set; }

		public double MinRatio { get; private set; }
	}
}