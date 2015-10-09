using BotEngine;
using BotEngine.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sanderling.Parse
{
	static public class Ore
	{
		static public string RegexPattern(this OreTypeEnum OreType) =>
			System.Text.RegularExpressions.Regex.Replace(OreType.ToString(), "[\\s_]+", "[\\s]*");

		static Regex Regex(OreTypeEnum OreType) =>
			RegexPattern(OreType).AsRegexCompiledIgnoreCase();

		static readonly Cache<OreTypeEnum, Regex> CacheFromOreTypeToRegex = new Cache<OreTypeEnum, Regex>(Regex);

		static public OreTypeEnum? AsOreTypeEnum(this string OreTypeString) =>
			Bib3.Extension.EnumGetValues<OreTypeEnum>()
			?.FirstOrDefault(OreType => CacheFromOreTypeToRegex?.Retrieve(OreType)?.MatchSuccess(OreTypeString) ?? false);

	}
}
