using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	public interface IInventoryCapacityGauge
	{
		long? Used { get; }
		long? Max { get; }
		long? Selected { get; }
	}

	public interface IInventoryTreeViewEntryShip : MemoryStruct.ITreeViewEntry
	{
		IEnumerable<KeyValuePair<ShipCargoSpaceTypeEnum, MemoryStruct.ITreeViewEntry>> SetCargoSpaceTypeAndTreeEntry { get; }
	}

	public interface IWindowInventory : MemoryStruct.IWindowInventory
	{
		IInventoryTreeViewEntryShip ActiveShipEntry { get; }

		IInventoryCapacityGauge SelectedRightInventoryCapacityMilli { get; }

		ShipCargoSpaceTypeEnum? ActiveShipSelectedCargoSpaceTypeEnum { get; }

		MemoryStruct.ITreeViewEntry ItemHangarEntry { get; }

		int? SelectedRightItemFilteredCount { get; }

		int? SelectedRightItemDisplayedCount { get; }
	}

	public class WindowInventoryTreeViewShip : IInventoryTreeViewEntryShip
	{
		public MemoryStruct.ITreeViewEntry Raw { private set; get; }

		public IEnumerable<KeyValuePair<ShipCargoSpaceTypeEnum, MemoryStruct.ITreeViewEntry>> SetCargoSpaceTypeAndTreeEntry { private set; get; }

		public MemoryStruct.ITreeViewEntry[] Child => Raw?.Child;

		public IEnumerable<MemoryStruct.IUIElementText> ButtonText => Raw?.ButtonText;

		public IEnumerable<MemoryStruct.IUIElementInputText> InputText => Raw?.InputText;

		public IEnumerable<MemoryStruct.IUIElementText> LabelText => Raw?.LabelText;

		public IEnumerable<MemoryStruct.ISprite> Sprite => Raw?.Sprite;

		public bool? IsSelected => Raw?.IsSelected;

		public MemoryStruct.IUIElement ExpandToggleButton => Raw?.ExpandToggleButton;

		public bool? IsExpanded => Raw?.IsExpanded;

		public string Text => Raw?.Text;

		public RectInt Region => Raw?.Region ?? RectInt.Empty;

		public int? InTreeIndex => Raw?.InTreeIndex;

		public MemoryStruct.IUIElement RegionInteraction => Raw?.RegionInteraction;

		public long Id => Raw?.Id ?? 0;

		public int? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		WindowInventoryTreeViewShip()
		{ }

		public WindowInventoryTreeViewShip(MemoryStruct.ITreeViewEntry raw)
		{
			this.Raw = raw;

			if (null == raw)
			{
				return;
			}

			SetCargoSpaceTypeAndTreeEntry = raw.FromShipExtractSetCargoSpaceTypeAndTreeEntry()?.ToArray();
		}
	}

	public class WindowInventory : IWindowInventory
	{
		public MemoryStruct.IWindowInventory Raw { private set; get; }

		public IEnumerable<MemoryStruct.IUIElementText> ButtonText => Raw?.ButtonText;

		public string Caption => Raw?.Caption;

		public MemoryStruct.ISprite[] HeaderButton => Raw?.HeaderButton;

		public bool? HeaderButtonsVisible => Raw?.HeaderButtonsVisible;

		public long Id => Raw?.Id ?? 0;

		public IEnumerable<MemoryStruct.IUIElementInputText> InputText => Raw?.InputText;

		public int? InTreeIndex => Raw?.InTreeIndex;

		public bool? isModal => Raw?.isModal;

		public IEnumerable<MemoryStruct.IUIElementText> LabelText => Raw?.LabelText;

		public MemoryStruct.ITreeViewEntry[] LeftTreeListEntry => Raw?.LeftTreeListEntry;

		public MemoryStruct.IScroll LeftTreeViewportScroll => Raw?.LeftTreeViewportScroll;

		public RectInt Region => Raw?.Region ?? RectInt.Empty;

		public MemoryStruct.IUIElement RegionInteraction => Raw?.RegionInteraction;

		public MemoryStruct.ISprite[] SelectedRightControlViewButton => Raw?.SelectedRightControlViewButton;

		public MemoryStruct.IUIElement SelectedRightFilterButtonClear => Raw?.SelectedRightFilterButtonClear;

		public MemoryStruct.IUIElementInputText SelectedRightFilterTextBox => Raw?.SelectedRightFilterTextBox;

		public MemoryStruct.IInventory SelectedRightInventory => Raw?.SelectedRightInventory;

		public MemoryStruct.IUIElementText SelectedRightInventoryCapacity => Raw?.SelectedRightInventoryCapacity;

		public MemoryStruct.IUIElementText SelectedRightInventoryPathLabel => Raw?.SelectedRightInventoryPathLabel;

		public IEnumerable<MemoryStruct.ISprite> Sprite => Raw?.Sprite;

		public IInventoryTreeViewEntryShip ActiveShipEntry { set; get; }

		public IInventoryCapacityGauge SelectedRightInventoryCapacityMilli { set; get; }

		public ShipCargoSpaceTypeEnum? ActiveShipSelectedCargoSpaceTypeEnum =>
			ActiveShipEntry?.SetCargoSpaceTypeAndTreeEntry?.KeyMap(Bib3.Extension.ToNullable)
			?.FirstOrDefault(cargoSpaceTypeAndTreeEntry => cargoSpaceTypeAndTreeEntry.Value?.IsSelected ?? false).Key;

		public MemoryStruct.ITreeViewEntry ItemHangarEntry { set; get; }

		public int? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		public int? SelectedRightItemFilteredCount =>
			(Raw as MemoryStruct.WindowInventory)?.SelectedRightItemFilteredCount;

		public int? SelectedRightItemDisplayedCount =>
			(Raw as MemoryStruct.WindowInventory)?.SelectedRightItemDisplayedCount;

		WindowInventory()
		{ }

		public WindowInventory(MemoryStruct.IWindowInventory raw)
		{
			this.Raw = raw;

			if (null == raw)
			{
				return;
			}

			ActiveShipEntry = raw.TreeEntryActiveShip()?.ParseAsInventoryTreeEntryShip();

			SelectedRightInventoryCapacityMilli = raw?.SelectedRightInventoryCapacity?.Text?.ParseAsInventoryCapacityGaugeMilli();

			ItemHangarEntry = raw?.LeftTreeListEntry?.FirstOrDefault(c => c?.Text?.RegexMatchSuccess(@"item\s*hangar", RegexOptions.IgnoreCase) ?? false);
		}
	}

	public class InventoryCapacityGauge : IInventoryCapacityGauge
	{
		public long? Used { set; get; }
		public long? Max { set; get; }
		public long? Selected { set; get; }

		public InventoryCapacityGauge()
		{
		}

		public override bool Equals(object obj) =>
			base.Equals(obj) ||
			(obj is InventoryCapacityGauge &&
			(obj as InventoryCapacityGauge)?.Used == Used &&
			(obj as InventoryCapacityGauge)?.Max == Max &&
			(obj as InventoryCapacityGauge)?.Selected == Selected);

		public override int GetHashCode()
		{
			return Used.GetHashCode() ^ Max.GetHashCode() ^ Selected.GetHashCode();
		}
	}

	static public class InventoryExtension
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

		static public IInventoryCapacityGauge ParseAsInventoryCapacityGaugeMilli(this string gaugeString)
		{
			if (null == gaugeString)
			{
				return null;
			}

			var Match = Regex.Match(gaugeString, CapacityGaugeTextPattern, RegexOptions.IgnoreCase);

			if (!Match.Success)
			{
				return null;
			}

			var Used = Number.NumberParseDecimalMilli(Match.Groups[CapacityGaugeGroupUsedId].Value);
			var Max = Number.NumberParseDecimalMilli(Match.Groups[CapacityGaugeGroupMaxId].Value);
			var Selected = Number.NumberParseDecimalMilli(Match.Groups[CapacityGaugeGroupSelectedId].Value);

			return new InventoryCapacityGauge()
			{
				Used = Used,
				Max = Max,
				Selected = Selected,
			};
		}

		static public ShipCargoSpaceTypeEnum? FromIventoryLabelParseShipCargoSpaceType(
			this string shipCargoSpaceTypeLabel) =>
			InventoryCargoTypeAndSetLabel?.Where(cargoSpaceTypeAndSetLabel =>
			cargoSpaceTypeAndSetLabel.Value?.Any(label => label.EqualsIgnoreCase(shipCargoSpaceTypeLabel)) ?? false)
			?.CastToNullable()?.FirstOrDefault()?.Key;

		static public IEnumerable<KeyValuePair<ShipCargoSpaceTypeEnum, MemoryStruct.ITreeViewEntry>> FromShipExtractSetCargoSpaceTypeAndTreeEntry(
			this MemoryStruct.ITreeViewEntry shipTreeEntry) =>
			new[] { new KeyValuePair<ShipCargoSpaceTypeEnum, MemoryStruct.ITreeViewEntry>(ShipCargoSpaceTypeEnum.General, shipTreeEntry) }
			.ConcatNullable(InventoryCargoTypeAndSetLabel?.Select(cargoTypeAndSetLabel =>
			new KeyValuePair<ShipCargoSpaceTypeEnum, MemoryStruct.ITreeViewEntry>(
				cargoTypeAndSetLabel.Key,
				shipTreeEntry?.EnumerateChildNodeTransitive()
				?.FirstOrDefault(node => node?.Text?.FromIventoryLabelParseShipCargoSpaceType() == cargoTypeAndSetLabel.Key))))
			?.Where(treeEntryForCargoSpaceType => null != treeEntryForCargoSpaceType.Value);

		static public MemoryStruct.ITreeViewEntry TreeEntryFromCargoSpaceType(
			this MemoryStruct.ITreeViewEntry shipTreeEntry,
			ShipCargoSpaceTypeEnum cargoSpaceType) =>
			FromShipExtractSetCargoSpaceTypeAndTreeEntry(shipTreeEntry)
			?.FirstOrDefault(treeEntryForCargoShipType => treeEntryForCargoShipType.Key == cargoSpaceType).Value;

		static public ShipCargoSpaceTypeEnum? CargoSpaceTypeFromTreeEntry(
			this MemoryStruct.ITreeViewEntry entryShip,
			MemoryStruct.ITreeViewEntry entryCargoSpace) =>
			FromShipExtractSetCargoSpaceTypeAndTreeEntry(entryShip)
			?.Where(cargoShipTypeAndTreeEntry => cargoShipTypeAndTreeEntry.Value == entryCargoSpace)
			?.Select(cargoShipTypeAndTreeEntry => cargoShipTypeAndTreeEntry.Key)
			?.CastToNullable()
			?.FirstOrDefault();

		static public KeyValuePair<string, string>? ParseTreeEntryLabelShipNameAndType(
			this string label)
		{
			var Match = Regex.Match(label, TreeEntryShipRegexPattern);

			if (!Match.Success)
			{
				return null;
			}

			return new KeyValuePair<string, string>(
				Match.Groups[TreeEntryShipGroupNameId].Value?.Trim(),
				Match.Groups[TreeEntryShipGroupTypeId].Value?.Trim());
		}

		static public MemoryStruct.ITreeViewEntry TreeEntryActiveShip(
			this MemoryStruct.IWindowInventory inventory) =>
			//	Topmost entry which is a root and has a conforming Label.
			inventory?.LeftTreeListEntry?.OrderByCenterVerticalDown()
			?.FirstOrDefault(treeEntry => 0 < treeEntry?.Text?.ParseTreeEntryLabelShipNameAndType()?.Value?.Length);

		static public IEnumerable<MemoryStruct.ITreeViewEntry> SetTreeEntrySuitingSelectedPath(
			this IEnumerable<MemoryStruct.ITreeViewEntry> setTreeEntryRoot,
			IEnumerable<string> selectedPathListNodeLabel)
		{
			if (null == setTreeEntryRoot || null == selectedPathListNodeLabel)
			{
				return null;
			}

			var PathListNodeLabelLessFormatting =
				selectedPathListNodeLabel?.Select(label => label?.RemoveXmlTag()?.Trim())?.ToArray();

			var PathListNodeLabelNext =
				PathListNodeLabelLessFormatting?.FirstOrDefault();

			if (null == PathListNodeLabelNext)
			{
				return null;
			}

			var SetTreeEntryRootSuitingPathNodeNext =
				setTreeEntryRoot
				?.Where(treeEntry => (treeEntry?.Text?.RemoveXmlTag()?.Trim()).EqualsIgnoreCase(PathListNodeLabelNext))
				?.ToArray();

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
				.Select((linxTreeEntryPasend) =>
					SetTreeEntrySuitingSelectedPath(
						linxTreeEntryPasend.Child,
						selectedPathListNodeLabel.Skip(1)))
				?.WhereNotDefault()
				?.ConcatNullable();
		}

		static public IEnumerable<MemoryStruct.ITreeViewEntry> SetLeftTreeEntrySelected(
			this MemoryStruct.IWindowInventory windowInventory) =>
			windowInventory?.LeftTreeListEntry?.Select(rootTreeEntry => rootTreeEntry?.EnumerateChildNodeTransitive()).ConcatNullable()
			?.Where(treeEntry => treeEntry?.IsSelected ?? false);

		static public ShipCargoSpaceTypeEnum? ActiveShipSelectedCargoSpaceType(
			this MemoryStruct.IWindowInventory windowInventory) =>
			windowInventory?.TreeEntryActiveShip()?.FromShipExtractSetCargoSpaceTypeAndTreeEntry()
			?.Where(cargoTypeAndTreeEntry => windowInventory?.SetLeftTreeEntrySelected()?.Contains(cargoTypeAndTreeEntry.Value) ?? false)
			?.Select(cargoTypeAndTreeEntry => cargoTypeAndTreeEntry.Key)
			?.CastToNullable()
			?.FirstOrDefault();

		static public IInventoryTreeViewEntryShip ParseAsInventoryTreeEntryShip(this MemoryStruct.ITreeViewEntry treeViewEntry) =>
			null == treeViewEntry ? null : new WindowInventoryTreeViewShip(treeViewEntry);

		static public IWindowInventory Parse(this MemoryStruct.IWindowInventory windowInventory) =>
			null == windowInventory ? null : new WindowInventory(windowInventory);
	}
}
