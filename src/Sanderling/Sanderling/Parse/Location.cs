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

		static public bool LocationEquals(this ILocation l0, ILocation l1)
		{
			if (ReferenceEquals(l0, l1))
				return true;

			if (null == l0 || null == l1)
				return false;

			return
				l0.SystemName == l1.SystemName &&
				l0.PlanetNumber == l1.PlanetNumber &&
				l0.MoonNumber == l1.MoonNumber;
		}

		static public ILocation AsLocation(this string locationText)
		{
			var TopMatch = locationText.RegexMatchIfSuccess(LocationRegexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

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
