using Bib3;
using Bib3.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Sanderling.Test.Exe
{
	static class General
	{
		static public void AssertSuccess<InT, OutT, ComparerT>(
			this TestCaseMap<InT, OutT, ComparerT> testCase,
			Func<InT, OutT> map)
			where ComparerT : IEqualityComparer<OutT>
		{
			Assert.IsTrue(testCase.Success(map));
		}

		static public void AssertSuccess<InT, OutT, ComparerT>(
			this IEnumerable<TestCaseMap<InT, OutT, ComparerT>> setTestCase,
			Func<InT, OutT> map)
			where ComparerT : IEqualityComparer<OutT>
		{
			foreach (var TestCase in setTestCase.EmptyIfNull())
			{
				try
				{
					TestCase.AssertSuccess(map);
				}
				catch (Exception Exception)
				{
					throw new ApplicationException("failed for test case: " + (TestCase?.ToString() ?? ""), Exception);
				}
			}
		}

		static public void AssertMapEquals<InT, OutT>(
			this IEnumerable<(InT, OutT)> testCases,
			Func<InT, OutT> map,
			IEqualityComparer<OutT> comparer)
		{
			foreach (var (input, expected) in testCases.EmptyIfNull())
			{
				try
				{
					var output = map(input);

					Assert.That(comparer.Equals(output, expected));
				}
				catch (Exception Exception)
				{
					throw new ApplicationException($"failed for test case with input = { input } and expected output = { expected }", Exception);
				}
			}
		}

		static public void AssertObjectEquals<InT, OutT>(
			this IEnumerable<KeyValuePair<InT, OutT>> setTestCase,
			Func<InT, OutT> map)
		{
			foreach (var testCase in setTestCase.EmptyIfNull())
			{
				try
				{
					var actual = map(testCase.Key);

					Assert.That(object.Equals(testCase.Value, actual));
				}
				catch (Exception Exception)
				{
					throw new ApplicationException("failed for test case: " + (testCase.ToString() ?? ""), Exception);
				}
			}
		}
	}
}
