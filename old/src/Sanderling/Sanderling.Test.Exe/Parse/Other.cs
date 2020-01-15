using NUnit.Framework;
using Sanderling.Parse;
using System;
using System.Collections.Generic;

namespace Sanderling.Test.Exe.Parse
{
	class Other
	{
		[Test]
		public void SecondCountFromBracketTimerText()
		{
			foreach (var testCase in new[]
			{
				new KeyValuePair<string, int?>("4m 47s", 4 * 60 + 47),
				new KeyValuePair<string, int?>("4 s", 4),
			})
			{
				try
				{
					Assert.AreEqual(testCase.Value, testCase.Key.SecondCountFromBracketTimerText());
				}
				catch (Exception exception)
				{
					throw new Exception("failed for case " + testCase.ToString(), exception);
				}
			}
		}
	}
}
