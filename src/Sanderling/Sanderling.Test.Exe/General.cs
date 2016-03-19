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
			this TestCaseMap<InT, OutT, ComparerT> TestCase,
			Func<InT, OutT> Map)
			where ComparerT : IEqualityComparer<OutT>
		{
			Assert.IsTrue(TestCase.Success(Map));
		}

		static public void AssertSuccess<InT, OutT, ComparerT>(
			this IEnumerable<TestCaseMap<InT, OutT, ComparerT>> SetTestCase,
			Func<InT, OutT> Map)
			where ComparerT : IEqualityComparer<OutT>
		{
			foreach (var TestCase in SetTestCase.EmptyIfNull())
			{
				try
				{
					TestCase.AssertSuccess(Map);
				}
				catch (Exception Exception)
				{
					throw new ApplicationException("failed for test case: " + (TestCase?.ToString() ?? ""), Exception);
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
