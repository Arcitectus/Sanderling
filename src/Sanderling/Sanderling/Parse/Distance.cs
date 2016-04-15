using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BotEngine.Common;

namespace Sanderling.Parse
{
	static public class Distance
	{
		/// <summary>
		/// CCPs AU actually not measured yet, this should be close enough.
		/// http://www.nature.com/news/the-astronomical-unit-gets-fixed-1.11416
		/// </summary>
		public const Int64 AstronomicalUnit = 149597870700;

		public const string DistanceRegexPatternGroupValueId = "value";
		public const string DistanceRegexPatternGroupUnitId = "unit";

		public const string DistanceComponentUnitRegexPattern = @"\w+";

		readonly static public string DistanceRegexPattern =
			@"(?<" + DistanceRegexPatternGroupValueId + ">" +
			Number.DefaultNumberFormatRegexPatternConstruct(true, true) +
			@")\s*" +
			@"(?<" + DistanceRegexPatternGroupUnitId + ">" +
			DistanceComponentUnitRegexPattern +
			")";

		readonly static public Regex DistanceRegex = DistanceRegexPattern.AsRegexCompiled();

		static public void DistanceParse(
			string distanceString,
			out Int64? distanceMin,
			out Int64? distanceMax)
		{
			distanceMin = null;
			distanceMax = null;

			if (null == distanceString)
			{
				return;
			}

			var Match = DistanceRegex.Match(distanceString);

			if (!Match.Success)
			{
				return;
			}

			var ComponentValueString = Match.Groups[DistanceRegexPatternGroupValueId].Value;
			var ComponentUnitString = Match.Groups[DistanceRegexPatternGroupUnitId].Value;

			Int64? UnitInMeter = null;

			if (string.Equals("m", ComponentUnitString))
			{
				UnitInMeter = 1;
			}

			if (string.Equals("km", ComponentUnitString))
			{
				UnitInMeter = 1000;
			}

			if (string.Equals("AU", ComponentUnitString))
			{
				UnitInMeter = AstronomicalUnit;
			}

			var ValueMilli = Number.NumberParseDecimalMilli(ComponentValueString);

			var DistanceParsed = (ValueMilli * UnitInMeter) / 1000;

			//	eventually lower and upper bounds should be determined by NumberParseValueMilli but that is not implemented yet.
			//	assume zero fraction digits as default.
			var DiffBetweenLowerAndUpperBound = UnitInMeter;

			if (1e+6 < UnitInMeter)
			{
				//	assume one fraction digit for unit AU.
				DiffBetweenLowerAndUpperBound = UnitInMeter / 10;
			}

			//	Eve Online Client seems to always round down.
			distanceMin = DistanceParsed;
			distanceMax = distanceMin + DiffBetweenLowerAndUpperBound;
		}

		static public Int64? DistanceParseMin(
			this string distanceString)
		{
			Int64? DistanceMin;
			Int64? DistanceMax;

			DistanceParse(distanceString, out DistanceMin, out DistanceMax);

			return DistanceMin;
		}

		static public Int64? DistanceParseMax(
			this string distanceString)
		{
			Int64? DistanceMin;
			Int64? DistanceMax;

			DistanceParse(distanceString, out DistanceMin, out DistanceMax);

			return DistanceMax;
		}
		static public KeyValuePair<Int64, Int64>? DistanceParseMinMaxKeyValue(
			this string distanceString)
		{
			Int64? DistanceMin;
			Int64? DistanceMax;

			DistanceParse(distanceString, out DistanceMin, out DistanceMax);

			if (!DistanceMin.HasValue || !DistanceMax.HasValue)
			{
				return null;
			}

			return new KeyValuePair<Int64, Int64>(DistanceMin.Value, DistanceMax.Value);
		}
	}
}
