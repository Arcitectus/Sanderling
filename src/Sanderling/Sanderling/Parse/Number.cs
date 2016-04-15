using Bib3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sanderling.Parse
{
	/// <summary>
	/// Number formatting in the eve online client depends on number format configured in windows.
	/// For this reason, the number parsing works with a range of different group separators and decimal separators.
	/// </summary>
	static public class Number
	{
		const string InNumberRegexPatternSignGroupName = "Sign";
		const string InNumberRegexPatternPreDecimalSeparatorGroupName = "PreDecimalSeparator";
		const string InNumberRegexPatternPostDecimalSeparatorGroupName = "PostDecimalSeparator";
		const string InNumberRegexPatternDecimalSeparatorGroupName = "DecimalSeparator";
		const string InNumberRegexPatternDigitGroupSeparatorGroupName = "DigitGroupSeparator";

		static readonly public string DefaultNumberFormatRegexPattern = DefaultNumberFormatRegexPatternConstruct();
		static readonly public string DefaultNumberFormatRegexPatternAllowLeadingAndTrailingChars = DefaultNumberFormatRegexPatternConstruct(allowLeadingCharacters: true, allowTrailingCharacters: true);

		static readonly public Regex DefaultNumberFormatRegex =
			new Regex(DefaultNumberFormatRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

		static readonly public Regex DefaultNumberFormatRegexAllowLeadingAndTrailingChars =
			new Regex(DefaultNumberFormatRegexPatternAllowLeadingAndTrailingChars, RegexOptions.IgnoreCase | RegexOptions.Compiled);

		static public string DefaultNumberFormatRegexPatternConstruct(
			bool allowLeadingCharacters = false,
			bool allowTrailingCharacters = false,
			string groupIdSufix = null)
		{
			return
				NumberFormatRegexPatternConstruct(
					new string[] { "+", "-" },
					new string[] { ".", "," },
					new string[] { ".", ",", "'", " ", ""
						//	،: "Pashto" (ps)
						,"،"
					},
					allowLeadingCharacters,
					allowTrailingCharacters,
					groupIdSufix);
		}

		static public string RegexPatternAlternativeConstruct(
			string[] setOption)
		{
			if (null == setOption)
			{
				return null;
			}

			var SetCandidateEscaped =
				setOption
				.WhereNotDefault()
				.Select(candidate =>
				{
					if (0 < candidate.Length && candidate?.Trim().Length < 1)
					{
						return @"\s";
					}

					return Regex.Escape(candidate);
				}).ToArray();

			return
				"(" +
				string.Join(
				"|",
				SetCandidateEscaped) +
				")";
		}

		static public string NumberFormatRegexPatternConstruct(
			string[] setSignOption,
			string[] setDecimalSeparatorOption,
			string[] setDigitGroupSeparatorOption,
			bool allowLeadingCharacters = false,
			bool allowTrailingCharacters = false,
			string groupIdSufix = null)
		{
			var InNumberRegexPatternDigitGroupSeparatorGroupName =
				Number.InNumberRegexPatternDigitGroupSeparatorGroupName + (groupIdSufix ?? "");

			var InNumberRegexPatternSignGroupName =
				Number.InNumberRegexPatternSignGroupName + (groupIdSufix ?? "");

			var InNumberRegexPatternPreDecimalSeparatorGroupName =
				Number.InNumberRegexPatternPreDecimalSeparatorGroupName + (groupIdSufix ?? "");

			setSignOption = (setSignOption ?? new string[0]).WhereNotDefault().Concat(new string[] { "" }).ToArray();

			setDigitGroupSeparatorOption = (setDigitGroupSeparatorOption ?? new string[0]).WhereNotDefault().ToArray();

			var PatternSign = RegexPatternAlternativeConstruct(setSignOption);

			var PatternDecimalSeparator = "(?!\\<" + InNumberRegexPatternDigitGroupSeparatorGroupName + ">)" + RegexPatternAlternativeConstruct(setDecimalSeparatorOption);
			var PatternDigitGroupSeparator = RegexPatternAlternativeConstruct(setDigitGroupSeparatorOption);

			//	Grupe direkt vor Dezimaltrenzaice mus drai Zifern enthalte. Grupe Linx davon dürfe zwai oder drai Zifern enthalte.
			//	d.h. inerhalb der optionaale Grupe welce ale Zaice zwisce inklusiiv früühescte Grupetrenzaice und Dezimaltrenzaice enthalt
			//	isc zuusäzlic auf linker Saite optionaale Grupe für Ziferngrupe mit variabler Anzaal von Zifern enthalte.
			//	da di Grupe welce das Zaice für Ziferngrupiirung ersctmaals fängt linx von ale andere vorkome des Ziferngrupiirungszaice scteehe sol werd inerhalb
			//	der optionaale Grupe di Folge von Ziferngrupe und Ziferngrupiirungszaice umgekeert.
			var TailVorDezimaaltrenzaiceTailOptioonTailOptioonVarAnz =
				@"(\d{2,3}" +
				@"\<" + InNumberRegexPatternDigitGroupSeparatorGroupName + @">)*";

			var TailVorDezimaaltrenzaiceTailOptioon =
				"((?<" + InNumberRegexPatternDigitGroupSeparatorGroupName + ">" + PatternDigitGroupSeparator + @")" +
				TailVorDezimaaltrenzaiceTailOptioonTailOptioonVarAnz +
				@"\d{3})";

			var PatternPreDecimalSeparator =
				@"\d+" +
				@"((" + TailVorDezimaaltrenzaiceTailOptioon + ")|)";

			//	post decimal seperator: allow for any number of digits except three.
			var PatternPostDecimalSeparator = @"(\d{0,2}|\d{4,})";

			var PatternBegin = allowLeadingCharacters ? "" : "^\\s*";

			var PatternEnd =
				//	prevent trailing digits (negative lookahead)
				@"(?!\d)" +
				(allowTrailingCharacters ? "" : "\\s*$");

			return
				PatternBegin +
				"(?<" + InNumberRegexPatternSignGroupName + ">" +
				PatternSign + ")" +

				//	spaces between sign and value.
				@"\s*" +

				"(?<" + InNumberRegexPatternPreDecimalSeparatorGroupName + ">" +
				PatternPreDecimalSeparator + ")" +
				"(|(?<" + InNumberRegexPatternDecimalSeparatorGroupName + ">" + PatternDecimalSeparator + ")" +
				"(?<" + InNumberRegexPatternPostDecimalSeparatorGroupName + ">" +
				PatternPostDecimalSeparator +
				"))" +
				PatternEnd;
		}

		/// <summary>
		/// parses a decimal number and returns the number multiplied by thousand.
		/// </summary>
		/// <param name="numberString"></param>
		/// <returns></returns>
		static public Int64? NumberParseDecimalMilli(this string numberString)
		{
			if (null == numberString)
			{
				return null;
			}

			var RegexMatch = DefaultNumberFormatRegex.Match(numberString.Trim());

			if (!(RegexMatch?.Success ?? false))
			{
				return null;
			}

			var Sign = 1;

			var SignString = RegexMatch.Groups[InNumberRegexPatternSignGroupName].Value;

			if ("-" == SignString)
			{
				Sign = -1;
			}

			var PreDecimalSeparator = RegexMatch.Groups[InNumberRegexPatternPreDecimalSeparatorGroupName].Value;
			var PostDecimalSeparator = RegexMatch.Groups[InNumberRegexPatternPostDecimalSeparatorGroupName].Value;

			var PreDecimalSeparatorLessSeparator = Regex.Replace(PreDecimalSeparator, "[^\\d]+", "");
			var PostDecimalSeparatorLessSeparator = Regex.Replace(PostDecimalSeparator, "[^\\d]+", "");

			var PreDecimalSeparatorValue =
				0 < PreDecimalSeparatorLessSeparator.Length ?
				Int64.Parse(PreDecimalSeparatorLessSeparator) :
				0;

			var PostDecimalSeparatorValueMikro =
				0 < PostDecimalSeparatorLessSeparator.Length ?
				(Int64)(Int64.Parse(PostDecimalSeparatorLessSeparator) * Math.Pow(10, 6 - PostDecimalSeparatorLessSeparator.Length)) :
				0;

			return Sign * (PreDecimalSeparatorValue * 1000 + PostDecimalSeparatorValueMikro / 1000);
		}

		static public Int64? NumberParseDecimal(this string numberString) =>
			NumberParseDecimalMilli(numberString) / 1000;

		private enum RomanDigitValue
		{
			I = 1,
			V = 5,
			X = 10,
			L = 50,
			C = 100,
			D = 500,
			M = 1000
		}

		static public int? IntFromRoman(this string roman)
		{
			if (roman.IsNullOrEmpty())
				return null;

			roman = roman.Trim().ToUpper();

			if (roman == "N")
				return 0;

			// Rule 4
			if (roman.Split('V').Length > 2 ||
				roman.Split('L').Length > 2 ||
				roman.Split('D').Length > 2)
				return null;

			// Rule 1
			int count = 1;
			char last = 'Z';
			foreach (char numeral in roman)
			{
				// Valid character?
				if ("IVXLCDM".IndexOf(numeral) == -1)
					return null;

				// Duplicate?
				if (numeral == last)
				{
					count++;
					if (count == 4)
						return null;
				}
				else
				{
					count = 1;
					last = numeral;
				}
			}

			// Create an ArrayList containing the values
			int DigitIndex = 0;
			var ListDigitValue = new List<int>();
			int maxDigit = 1000;
			while (DigitIndex < roman.Length)
			{
				// Base value of digit
				char numeral = roman[DigitIndex];
				int DigitValue = (int)Enum.Parse(typeof(RomanDigitValue), numeral.ToString());

				// Rule 3
				if (DigitValue > maxDigit)
					return null;

				// Next digit
				int nextDigit = 0;
				if (DigitIndex < roman.Length - 1)
				{
					char nextNumeral = roman[DigitIndex + 1];
					nextDigit = (int)Enum.Parse(typeof(RomanDigitValue), nextNumeral.ToString());

					if (nextDigit > DigitValue)
					{
						if ("IXC".IndexOf(numeral) == -1 ||
							nextDigit > (DigitValue * 10) ||
							roman.Split(numeral).Length > 3)
							throw new ArgumentException("Rule 3");

						maxDigit = DigitValue - 1;
						DigitValue = nextDigit - DigitValue;
						DigitIndex++;
					}
				}

				ListDigitValue.Add(DigitValue);

				// Next digit
				DigitIndex++;
			}

			// Rule 5
			for (int i = 0; i < ListDigitValue.Count - 1; i++)
				if (ListDigitValue[i] < ListDigitValue[i + 1])
					return null;

			return ListDigitValue.Sum();
		}
	}
}
