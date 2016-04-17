﻿using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace CodeJam
{
	[TestFixture]
	public class UpperBoundTest
	{
		[Test]
		public void Test01NegativeFrom()
		{
			var list = new List<double> { 0 };
			const int from = -1;
			// comparer version
			Assert.That(() => list.UpperBound(10.0, from, list.Count, Comparer<double>.Default.Compare)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.UpperBound(10.0, from, list.Count)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.UpperBound(10.0, from, list.Count)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test02NegativeTo()
		{
			var list = new List<double> { 0 };
			const int to = -1;
			// comparer version
			Assert.That(() => list.UpperBound(10.0, 0, to, Comparer<double>.Default.Compare)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.UpperBound(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.UpperBound(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test03ToExceedsCount()
		{
			var list = new List<double> { 0 };
			var to = list.Count + 1;
			// comparer version
			Assert.That(() => list.UpperBound(10.0, 0, to, Comparer<double>.Default.Compare)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.UpperBound(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.UpperBound(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test04BadFromToOrder()
		{
			var list = new List<double> { 0, 1, 2 };
			const int from = 2;
			const int to = 1;
			// comparer version
			Assert.That(() => list.UpperBound(10.0, from, to, Comparer<double>.Default.Compare)
				, Throws.ArgumentException);
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.UpperBound(10.0, from, to), Throws.ArgumentException);
			// specific version
			Assert.That(() => list.UpperBound(10.0, from, to), Throws.ArgumentException);
		}

		[Test]
		[TestCase(new double[0], 1.0, 0, 0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 5.0, 0, 0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 21.0, 3, 3, 3)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 101.0, 5, 5, 5)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 15.0, 0, 3, 3)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 5.0, 1, 4, 2)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 30000.0, 0, 4, 4)]
		public void Test05WithAllParams(double[] data, double value, int from, int to, int expected)
		{
			// comparer version
			var list = (IList<double>)data;
			Assert.That(list.UpperBound(value, from, to, Comparer<double>.Default.Compare), Is.EqualTo(expected));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(list2.UpperBound(value, from, to), Is.EqualTo(expected));
			// specific version
			Assert.That(list.UpperBound(value, from, to), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(new double[0], 11.0, 0)]
		public void Test06WithComparer(double[] data, double value, int expected)
		{
			// comparer version
			var list = (IList<double>)data;
			Assert.That(list.UpperBound(value, Comparer<double>.Default.Compare), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(new double[0], 4.0, 0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 15.0, 5, 5)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 15.0, 0, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 5.0, 0, 2)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, -1.0, 4, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1.0, 3, 3)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 42.0, 6, 6)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1002.0, 3, 7)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 12.0, 1, 4)]
		public void Test07WithFrom(double[] data, double value, int from, int expected)
		{
			// comparer version
			var list = (IList<double>)data;
			Assert.That(list.UpperBound(value, from, Comparer<double>.Default.Compare), Is.EqualTo(expected));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(list2.UpperBound(value, from), Is.EqualTo(expected));
			// specific version
			Assert.That(list.UpperBound(value, from), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(new double[0], 14.0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 50.0, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 5.0, 2)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 30000.0, 8)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, -1.0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1.0, 1)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 42.0, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1002.0, 7)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 12.0, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 3.0, 1)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 14534.0, 8)]
		public void Test08WithoutParams(double[] data, double value, int expected)
		{
			var list = (IList<double>)data;
			Assert.That(list.UpperBound(value, Comparer<double>.Default.Compare), Is.EqualTo(expected));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(list2.UpperBound(value), Is.EqualTo(expected));
			// specific version
			Assert.That(list.UpperBound(value), Is.EqualTo(expected));
		}
	}
}
