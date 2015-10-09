using Bib3;
using Bib3.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}
