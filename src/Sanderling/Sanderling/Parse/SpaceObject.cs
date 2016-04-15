using Bib3;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sanderling.Parse
{
	static public class SpaceObject
	{
		public const string SpaceObjectMenuEntryRemoveFromOverviewRegexPattern = @"remove.*from\s*overview";

		public const string SpaceObjectMenuEntryLockRegexPattern = @"(?<!\w)lock";

		public const string SpaceObjectMenuEntryUnlockRegexPattern = @"unlock";

		public const string SpaceObjectMenuEntryApproachRegexPattern = "approach";

		static readonly public KeyValuePair<ShipManeuverTypeEnum, string>[] SpaceObjectSetManeuverTypeAndMenuEntryRegexPattern = new[]
		{
			new KeyValuePair<ShipManeuverTypeEnum, string>(ShipManeuverTypeEnum.Approach, SpaceObjectMenuEntryApproachRegexPattern),
		};

		/// <summary>
		/// Example:
		/// "Asteroid ( Concentrated Veldspar )"
		/// "Asteroid(Veldspar)"
		/// </summary>
		const string TargetLabelAsteroidRegexPattern = "Asteroid.*[^\\d\\w\\s]+([\\d\\w\\s]+)";

		static public IMenuEntry EntryLock(this IMenu menu) =>
			menu?.Entry?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase(SpaceObjectMenuEntryLockRegexPattern) ?? false);

		static public IMenuEntry EntryUnlock(this IMenu menu) =>
			menu?.Entry?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase(SpaceObjectMenuEntryUnlockRegexPattern) ?? false);

		static public IMenuEntry EntryRemoveFromOverview(this IMenu menu) =>
			menu?.Entry?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase(SpaceObjectMenuEntryRemoveFromOverviewRegexPattern) ?? false);

		static public KeyValuePair<IOverviewEntry, IEnumerable<IMenu>>? OverviewEntryMenu(this IMemoryMeasurement memoryMeasurement)
		{
			var OverviewEntry =
				memoryMeasurement?.WindowOverview
				?.Select(windowOverview => windowOverview?.ListView?.Entry)
				?.ConcatNullable()
				?.OfType<IOverviewEntry>()
				?.FirstOrDefault(entry => entry?.IsSelected ?? false);

			if (null == OverviewEntry)
			{
				return null;
			}

			if (null == memoryMeasurement?.Menu?.FirstOrDefault()?.EntryRemoveFromOverview())
			{
				return null;
			}

			return new KeyValuePair<IOverviewEntry, IEnumerable<IMenu>>(OverviewEntry, memoryMeasurement?.Menu);
		}

		static public string OreTypeString(this IOverviewEntry overviewEntry) =>
			(overviewEntry?.ColumnNameValue().RegexMatchSuccessIgnoreCase("Asteroid.*") ?? false) ?
			overviewEntry?.ColumnTypeValue() : null;

		static public OreTypeEnum? OreTypeEnum(this IOverviewEntry overviewEntry) =>
			Bib3.Extension.EnumGetValues<OreTypeEnum>()
			?.CastToNullable()
			?.FirstOrDefault(oreType => overviewEntry.OreTypeString().RegexMatchSuccessIgnoreCase(oreType?.RegexPattern()));

		static public OreTypeEnum? ExtractAsteroidOreType(
			this ShipUiTarget target,
			out string oreTypeString)
		{
			oreTypeString = null;

			var LabelAggregatedLessXml =
				target?.LabelText?.Select(label => label?.Text)?.StringJoin(" ")?.RemoveXmlTag()?.Trim();

			if (null == LabelAggregatedLessXml)
			{
				return null;
			}

			var Match = Regex.Match(
				LabelAggregatedLessXml, TargetLabelAsteroidRegexPattern, RegexOptions.IgnoreCase);

			if (!Match.Success)
			{
				return null;
			}

			oreTypeString = Match.Groups[1].Value.Trim();

			return oreTypeString.AsOreTypeEnum();
		}

		static public OreTypeEnum? ExtractAsteroidOreType(this ShipUiTarget target)
		{
			string OreTypeString;

			return ExtractAsteroidOreType(target, out OreTypeString);
		}

	}
}