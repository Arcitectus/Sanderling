using Bib3;
using NUnit.Framework;
using Sanderling.Parse.Test;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sanderling.Test.Exe.Parse
{
	static public class Number
	{
		static public void Test(IEnumerable<ParseNumberTestCase> setTestCase) =>
			setTestCase.AssertSuccess(Sanderling.Parse.Number.NumberParseDecimalMilli);

		static public void TestSupportedCulture() =>
			Test(Sanderling.Parse.Test.Number.NumberTestCaseCombine(Sanderling.Parse.Test.Number.SupportedCulture));

		static public void Test(CultureInfo culture) => Test(Sanderling.Parse.Test.Number.NumberTestCaseCombine(culture));

		[Test]
		static public void Parse_Number_TestSupportedCulture()
		{
			TestSupportedCulture();
		}

		[Test]
		static public void Parse_Number_Throws()
		{
			Assert.That(() => Test(new ParseNumberTestCase
			{
				In = "not a number",
				Out = 0,
				Culture = CultureInfo.InvariantCulture,
			}.Yield()),
			Throws.InstanceOf<ApplicationException>());
		}
	}
}
