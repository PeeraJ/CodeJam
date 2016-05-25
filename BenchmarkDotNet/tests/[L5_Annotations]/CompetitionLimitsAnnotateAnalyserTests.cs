using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running.Competitions.Core;
using BenchmarkDotNet.Running.Messages;

using NUnit.Framework;

namespace CodeJam.BenchmarkDotNet
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class CompetitionLimitsAnnotateAnalyserTests
	{
		private static readonly ICompetitionConfig _config = new ManualCompetitionConfig(FastRunConfig.Instance)
		{
			UpdateSourceAnnotations = true,
			DebugMode = true,
			EnableReruns = true,
			IgnoreExistingAnnotations = true,
			LogAnnotationResults = true,
			PreviousLogUri = "https://gist.githubusercontent.com/ig-sinicyn/075a36130b83b398ede5052d04d54f71/raw/78dbdfc79dc76a702026645eccf73705e9c52462/bench.net.log",
			//PreviousLogUri = "CompetitionLimitsAnnotateAnalyserTests.log.txt",
		};
		[Test]
		public static void TestAnnotateFromLog()
		{
			var stopwatch = Stopwatch.StartNew();
			var summary = CompetitionBenchmarkRunner.Run<HighAccuracyBenchmark>(_config);
			stopwatch.Stop();
			var runState = CompetitionCore.RunState[summary];
			var messages = runState.GetMessages();
			Assert.IsTrue(runState.MaxMessageSeverityInRun <= MessageSeverity.Warning);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.GreaterOrEqual(messages.Length, 5);
			Assert.LessOrEqual(stopwatch.Elapsed.TotalSeconds, 7);
		}

		#region Benchmark classes
		private const int SpinCount = 100 * 1000;

		public class HighAccuracyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => Thread.SpinWait(SpinCount);

			[CompetitionBenchmark]
			public void SlowerX10() => Thread.SpinWait(10 * SpinCount);
		}
		#endregion
	}
}