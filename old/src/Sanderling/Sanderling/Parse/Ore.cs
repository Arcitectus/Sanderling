using BotEngine;
using BotEngine.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sanderling.Parse
{
	static public class Ore
	{
		static public string RegexPattern(this OreTypeEnum oreType) =>
			System.Text.RegularExpressions.Regex.Replace(oreType.ToString(), "[\\s_]+", "[\\s]*");

		static Regex Regex(OreTypeEnum oreType) =>
			RegexPattern(oreType).AsRegexCompiledIgnoreCase();

		static readonly Cache<OreTypeEnum, Regex> CacheFromOreTypeToRegex = new Cache<OreTypeEnum, Regex>(Regex);

		static public OreTypeEnum? AsOreTypeEnum(this string oreTypeString) =>
			Bib3.Extension.EnumGetValues<OreTypeEnum>()
			?.FirstOrDefault(oreType => CacheFromOreTypeToRegex?.Retrieve(oreType)?.MatchSuccess(oreTypeString) ?? false);

	}
}
