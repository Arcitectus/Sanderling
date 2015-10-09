using Bib3;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BotEngine.EveOnline.Parse.Test
{
	public class ParseNumberTestCase
	{
		public string NumberString;

		public Int64 NumberValueMilli;

		public CultureInfo Culture;
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
			IEnumerable<Int64> SetNumberValueMilli,
			CultureInfo Culture) =>
			SetNumberValueMilli
				.Concat(SetNumberValueMilli.Select(Value => -Value))
				.Select(NumberValueMilli => new ParseNumberTestCase()
				{
					NumberValueMilli = NumberValueMilli,
					Culture = Culture,
					NumberString = (NumberValueMilli * 1e-3).ToString("N2", Culture),
				});

		static public IEnumerable<ParseNumberTestCase> NumberTestCaseCombine(CultureInfo Culture) =>
			NumberTestCaseCombine(NumberTestCaseValueMilli, Culture);

		static public IEnumerable<ParseNumberTestCase> NumberTestCaseCombine(
			IEnumerable<CultureInfo> SetCulture) =>
			SetCulture.Select(Culture => NumberTestCaseCombine(Culture)).ConcatNullable();

	}

}
