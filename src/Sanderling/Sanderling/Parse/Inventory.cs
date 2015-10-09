using Bib3;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sanderling.Parse
{
	public class InventoryCapacityGaugeNumeric
	{
		public long? Used;
		public long? Max;
		public long? Selected;

		public InventoryCapacityGaugeNumeric()
		{
		}

		public override bool Equals(object obj) =>
			base.Equals(obj) ||
			(obj is InventoryCapacityGaugeNumeric &&
			(obj as InventoryCapacityGaugeNumeric)?.Used == Used &&
			(obj as InventoryCapacityGaugeNumeric)?.Max == Max &&
			(obj as InventoryCapacityGaugeNumeric)?.Selected == Selected);

		public override int GetHashCode()
		{
			return Used.GetHashCode() ^ Max.GetHashCode() ^ Selected.GetHashCode();
		}
	}

	static public class Inventory
	{
		static readonly string CapacityGaugeUnitPattern = Regex.Escape("m³");

		/// <summary>
		/// //	،: "Pashto" (ps)
		/// </summary>
		static readonly string CapacityGaugeNumberChar = @"\d\.\,\'\s\،";

		const string CapacityGaugeGroupSelectedId = "selected";
		const string CapacityGaugeGroupUsedId = "used";
		const string CapacityGaugeGroupMaxId = "max";

		static readonly string CapacityGaugeTextGroupSelectedPattern = @"(\((?<" + CapacityGaugeGroupSelectedId + ">[" + CapacityGaugeNumberChar + @"]+)\)|)";

		static readonly string CapacityGaugeTextGroupUsedPattern = "(?<" + CapacityGaugeGroupUsedId + ">[" + CapacityGaugeNumberChar + "]+)";

		static readonly string CapacityGaugeTextGroupMaxPattern = @"(\/\s*(?<" + CapacityGaugeGroupMaxId + ">[" + CapacityGaugeNumberChar + "]+)|)";

		static readonly string CapacityGaugeTextPattern =
			CapacityGaugeTextGroupSelectedPattern + @"\s*" +
			CapacityGaugeTextGroupUsedPattern + @"\s*" +
			CapacityGaugeTextGroupMaxPattern + @"\s*" +
			CapacityGaugeUnitPattern;

		const string TreeEntryShipGroupNameId = "name";
		const string TreeEntryShipGroupTypeId = "type";

		const string TreeEntryShipNamePattern = @"[^\(\)]*";
		const string TreeEntryShipTypePattern = TreeEntryShipNamePattern;

		const string TreeEntryShipRegexPattern =
			@"(?<" + TreeEntryShipGroupNameId + @">" + TreeEntryShipNamePattern + @")\s*\(\s*(?<" +
			TreeEntryShipGroupTypeId + @">" + TreeEntryShipTypePattern + @")\s*\)";

		static public IEnumerable<KeyValuePair<ShipCargoSpaceTypeEnum, IEnumerable<string>>>
			InventoryCargoTypeAndSetLabel = new[]
			{
				new KeyValuePair<ShipCargoSpaceTypeEnum, IEnumerable<string>>(ShipCargoSpaceTypeEnum.DroneBay, new[] {"Drone Bay"}),
				new KeyValuePair<ShipCargoSpaceTypeEnum, IEnumerable<string>>(ShipCargoSpaceTypeEnum.OreHold, new[] {"Ore Hold"}),
			};


		static public InventoryCapacityGaugeNumeric ParseAsInventoryCapacityGaugeMilli(this string GaugeString)
		{
			if (null == GaugeString)
			{
				return null;
			}

			var Match = Regex.Match(GaugeString, CapacityGaugeTextPattern, RegexOptions.IgnoreCase);

			if (!Match.Success)
			{
				return null;
			}

			var Used = Number.NumberParseDecimalMilli(Match.Groups[CapacityGaugeGroupUsedId].Value);
			var Max = Number.NumberParseDecimalMilli(Match.Groups[CapacityGaugeGroupMaxId].Value);
			var Selected = Number.NumberParseDecimalMilli(Match.Groups[CapacityGaugeGroupSelectedId].Value);

			return new InventoryCapacityGaugeNumeric()
			{
				Used = Used,
				Max = Max,
				Selected = Selected,
			};
		}

		static public ShipCargoSpaceTypeEnum? FromIventoryLabelParseShipCargoSpaceType(
			this string ShipCargoSpaceTypeLabel) =>
			InventoryCargoTypeAndSetLabel?.Where(CargoSpaceTypeAndSetLabel =>
			CargoSpaceTypeAndSetLabel.Value?.Any(Label => Label.EqualsIgnoreCase(ShipCargoSpaceTypeLabel)) ?? false)
			?.CastToNullable()?.FirstOrDefault()?.Key;

		static public IEnumerable<KeyValuePair<ShipCargoSpaceTypeEnum, TreeViewEntry>> FromShipExtractSetCargoSpaceTypeAndTreeEntry(
			this TreeViewEntry ShipTreeEntry) =>
			new[] { new KeyValuePair<ShipCargoSpaceTypeEnum, TreeViewEntry>(ShipCargoSpaceTypeEnum.General, ShipTreeEntry) }
			.ConcatNullable(InventoryCargoTypeAndSetLabel?.Select(CargoTypeAndSetLabel =>
			new KeyValuePair<ShipCargoSpaceTypeEnum, TreeViewEntry>(
				CargoTypeAndSetLabel.Key,
				ShipTreeEntry?.EnumerateChildNodeTransitive()
				?.FirstOrDefault(Node => Node?.LabelText?.FromIventoryLabelParseShipCargoSpaceType() == CargoTypeAndSetLabel.Key))))
			?.Where(TreeEntryForCargoSpaceType => null != TreeEntryForCargoSpaceType.Value);

		static public TreeViewEntry TreeEntryForCargoSpaceType(
			this TreeViewEntry ShipTreeEntry,
			ShipCargoSpaceTypeEnum CargoSpaceType) =>
			FromShipExtractSetCargoSpaceTypeAndTreeEntry(ShipTreeEntry)
			?.FirstOrDefault(TreeEntryForCargoShipType => TreeEntryForCargoShipType.Key == CargoSpaceType).Value;

		static public ShipCargoSpaceTypeEnum? CargoSpaceTypeOfTreeEntry(
			this TreeViewEntry EntryShip,
			TreeViewEntry EntryCargoSpace) =>
			FromShipExtractSetCargoSpaceTypeAndTreeEntry(EntryShip)
			?.Where(CargoShipTypeAndTreeEntry => CargoShipTypeAndTreeEntry.Value == EntryCargoSpace)
			?.Select(CargoShipTypeAndTreeEntry => CargoShipTypeAndTreeEntry.Key)
			?.CastToNullable()
			?.FirstOrDefault();

		static public KeyValuePair<string, string>? ParseTreeEntryLabelShipNameAndType(
			this string Label)
		{
			var Match = Regex.Match(Label, TreeEntryShipRegexPattern);

			if (!Match.Success)
			{
				return null;
			}

			return new KeyValuePair<string, string>(
				Match.Groups[TreeEntryShipGroupNameId].Value?.Trim(),
				Match.Groups[TreeEntryShipGroupTypeId].Value?.Trim());
		}

		static public TreeViewEntry TreeEntryActiveShip(
			this WindowInventory Inventory) =>
			//	Topmost entry which is a root and has a conforming Label.
			Inventory?.LeftTreeListEntry?.OrderByCenterVerticalDown()
			?.FirstOrDefault(TreeEntry => 0 < TreeEntry?.LabelText?.ParseTreeEntryLabelShipNameAndType()?.Value?.Length);

		static public IEnumerable<TreeViewEntry> SetTreeEntrySuitingSelectedPath(
			this IEnumerable<TreeViewEntry> SetTreeEntryRoot,
			IEnumerable<string> SelectedPathListNodeLabel)
		{
			if (null == SetTreeEntryRoot || null == SelectedPathListNodeLabel)
			{
				return null;
			}

			var PathListNodeLabelLessFormatting =
				SelectedPathListNodeLabel?.Select(Label => Label?.RemoveXmlTag()?.Trim())?.ToArray();

			var PathListNodeLabelNext =
				PathListNodeLabelLessFormatting?.FirstOrDefault();

			if (null == PathListNodeLabelNext)
			{
				return null;
			}

			var SetTreeEntryRootSuitingPathNodeNext =
				SetTreeEntryRoot
				?.Where(TreeEntry => (TreeEntry?.LabelText?.RemoveXmlTag()?.Trim()).EqualsIgnoreCase(PathListNodeLabelNext))
				.ToArrayNullable();

			if (null == SetTreeEntryRootSuitingPathNodeNext)
			{
				return null;
			}

			if (PathListNodeLabelLessFormatting.Length < 2)
			{
				return SetTreeEntryRootSuitingPathNodeNext;
			}

			return
				SetTreeEntryRootSuitingPathNodeNext
				.Select((LinxTreeEntryPasend) =>
					SetTreeEntrySuitingSelectedPath(
						LinxTreeEntryPasend.Child,
						SelectedPathListNodeLabel.Skip(1)))
				?.Where((Kandidaat) => null != Kandidaat)
				?.ConcatNullable();
		}

		static public IEnumerable<TreeViewEntry> SetLeftTreeEntrySelected(
			this WindowInventory WindowInventory) =>
			WindowInventory?.LeftTreeListEntry?.Select(RootTreeEntry => RootTreeEntry?.EnumerateChildNodeTransitive()).ConcatNullable()
			?.Where(TreeEntry => TreeEntry?.IsSelected ?? false);

		static public ShipCargoSpaceTypeEnum? ActiveShipSelectedCargoSpaceType(
			this WindowInventory WindowInventory) =>
			WindowInventory?.TreeEntryActiveShip()?.FromShipExtractSetCargoSpaceTypeAndTreeEntry()
			?.Where(CargoTypeAndTreeEntry => WindowInventory?.SetLeftTreeEntrySelected()?.Contains(CargoTypeAndTreeEntry.Value) ?? false)
			?.Select(CargoTypeAndTreeEntry => CargoTypeAndTreeEntry.Key)
			?.CastToNullable()
			?.FirstOrDefault();

	}
}
