using Bib3;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sanderling.Parse.Test
{
	public class ParseNumberTestCase : Bib3.Test.TestCaseMapCompareByRefNezDif<string, Int64?>
	{
		public CultureInfo Culture;

		public override string ToString() =>
			"\"" + (In ?? "null") + "\" (Culture = " + (Culture?.ToString() ?? "");
	}

	static public class Number
	{
		static readonly Int64[] NumberTestCaseValueMilli = new[]
		{
			0L,
			40,
			440,
			4000,
			4400,
			4440,
			(Int64)134e+5,
			(Int64)134567e+5,
			(Int64)134567e+11
		};

		static public IEnumerable<CultureInfo> SupportedCulture => new[]
		{
			CultureInfo.GetCultureInfo("de"),
			CultureInfo.GetCultureInfo("en"),
			CultureInfo.GetCultureInfo("fr"),
			CultureInfo.GetCultureInfo("es"),
			CultureInfo.GetCultureInfo("ru"),
			//	CultureInfo.GetCultureInfo("ar"),	sign at the end of the number is currently not supported.
		};

		static public IEnumerable<ParseNumberTestCase> NumberTestCaseCombine(
			IEnumerable<Int64> setNumberValueMilli,
			CultureInfo culture) =>
			setNumberValueMilli
				.Concat(setNumberValueMilli.Select(value => -value))
				.Select(numberValueMilli => new ParseNumberTestCase()
				{
					In = (numberValueMilli * 1e-3).ToString("N2", culture),
					Out = numberValueMilli,
					Culture = culture,
				});

		static public IEnumerable<ParseNumberTestCase> NumberTestCaseCombine(CultureInfo culture) =>
			NumberTestCaseCombine(NumberTestCaseValueMilli, culture);

		static public IEnumerable<ParseNumberTestCase> NumberTestCaseCombine(
			IEnumerable<CultureInfo> setCulture) =>
			setCulture.Select(culture => NumberTestCaseCombine(culture)).ConcatNullable();

	}

}
