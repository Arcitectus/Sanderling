using Bib3;
using BotEngine.Common;

namespace Sanderling.Parse
{
	public interface ILocation
	{
		string SystemName { get; }

		int? PlanetNumber { get; }

		int? MoonNumber { get; }
	}

	public interface ILocationFromText : ILocation
	{
		string SystemAndPlanet { get; }
	}

	public class LocationFromText : ILocationFromText
	{
		public string SystemAndPlanet { set; get; }

		public string SystemName { set; get; }

		public int? PlanetNumber { set; get; }

		public int? MoonNumber { set; get; }
	}

	static public class LocationExtension
	{
		const string SystemRegexPattern = @"([^-]+?)(\s+[IVXL]+|)\s*$";
		const string MoonRegexPattern = @"M(oon|)\s*(\d+)";

		const string LocationRegexPattern = @"(?<system>[^-]+)(-\s*(?<moon>" + MoonRegexPattern + @")|)\s*(-|)";

		static public bool LocationEquals(this ILocation L0, ILocation L1)
		{
			if (ReferenceEquals(L0, L1))
				return true;

			if (null == L0 || null == L1)
				return false;

			return
				L0.SystemName == L1.SystemName &&
				L0.PlanetNumber == L1.PlanetNumber &&
				L0.MoonNumber == L1.MoonNumber;
		}

		static public ILocation AsLocation(this string LocationText)
		{
			var TopMatch = LocationText.RegexMatchIfSuccess(LocationRegexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			if (null == TopMatch)
				return null;

			var SystemAndPlanet = TopMatch?.Groups["system"]?.Value?.Trim();

			var SystemMatch = SystemAndPlanet?.RegexMatchIfSuccess(SystemRegexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			var SystemName = SystemMatch?.Groups[1]?.Value?.Trim();
			var PlanetNumber = SystemMatch?.Groups[2]?.Value?.Trim()?.IntFromRoman();

			var MoonMatch = TopMatch?.Groups["moon"]?.Value?.RegexMatchIfSuccess(MoonRegexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			var MoonNumber = MoonMatch?.Groups[2]?.Value?.TryParseInt();

			return new LocationFromText()
			{
				SystemAndPlanet = SystemAndPlanet,
				SystemName = SystemName,
				PlanetNumber = PlanetNumber,
				MoonNumber = MoonNumber,
			};
		}
	}
}
