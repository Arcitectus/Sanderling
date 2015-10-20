using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using System;

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
	}

	public class WindowInventoryTreeViewShip : IInventoryTreeViewEntryShip
	{
		public MemoryStruct.ITreeViewEntry Raw { private set; get; }

		public IEnumerable<KeyValuePair<ShipCargoSpaceTypeEnum, MemoryStruct.ITreeViewEntry>> SetCargoSpaceTypeAndTreeEntry { private set; get; }

		public MemoryStruct.ITreeViewEntry[] Child => Raw?.Child;

		public MemoryStruct.IUIElementText[] ButtonText => Raw?.ButtonText;

		public MemoryStruct.IUIElementInputText[] InputText => Raw?.InputText;

		public MemoryStruct.IUIElementText[] LabelText => Raw?.LabelText;

		public MemoryStruct.ISprite[] Sprite => Raw?.Sprite;

		public bool? IsSelected => Raw?.IsSelected;

		public MemoryStruct.IUIElement ExpandToggleButton => Raw?.ExpandToggleButton;

		public bool? IsExpanded => Raw?.IsExpanded;

		public string Text => Raw?.Text;

		public OrtogoonInt Region => Raw?.Region ?? OrtogoonInt.Empty;

		public int? InTreeIndex => Raw?.InTreeIndex;

		public MemoryStruct.IUIElement RegionInteraction => Raw?.RegionInteraction;

		public long Id => Raw?.Id ?? 0;

		public int? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		WindowInventoryTreeViewShip()
		{ }

		public WindowInventoryTreeViewShip(MemoryStruct.ITreeViewEntry Raw)
		{
			this.Raw = Raw;

			if (null == Raw)
			{
				return;
			}

			SetCargoSpaceTypeAndTreeEntry = Raw.FromShipExtractSetCargoSpaceTypeAndTreeEntry()?.ToArray();
		}
	}

	public class WindowInventory : IWindowInventory
	{
		public MemoryStruct.IWindowInventory Raw { private set; get; }

		public MemoryStruct.IUIElementText[] ButtonText => Raw?.ButtonText;

		public string Caption => Raw?.Caption;

		public MemoryStruct.ISprite[] HeaderButton => Raw?.HeaderButton;

		public bool? HeaderButtonsVisible => Raw?.HeaderButtonsVisible;

		public long Id => Raw?.Id ?? 0;

		public MemoryStruct.IUIElementInputText[] InputText => Raw?.InputText;

		public int? InTreeIndex => Raw?.InTreeIndex;

		public bool? isModal => Raw?.isModal;

		public MemoryStruct.IUIElementText[] LabelText => Raw?.LabelText;

		public MemoryStruct.ITreeViewEntry[] LeftTreeListEntry => Raw?.LeftTreeListEntry;

		public MemoryStruct.IScroll LeftTreeViewportScroll => Raw?.LeftTreeViewportScroll;

		public OrtogoonInt Region => Raw?.Region ?? OrtogoonInt.Empty;

		public MemoryStruct.IUIElement RegionInteraction => Raw?.RegionInteraction;

		public MemoryStruct.ISprite[] SelectedRightControlViewButton => Raw?.SelectedRightControlViewButton;

		public MemoryStruct.IUIElement SelectedRightFilterButtonClear => Raw?.SelectedRightFilterButtonClear;

		public MemoryStruct.IUIElementInputText SelectedRightFilterTextBox => Raw?.SelectedRightFilterTextBox;

		public MemoryStruct.IInventory SelectedRightInventory => Raw?.SelectedRightInventory;

		public MemoryStruct.IUIElementText SelectedRightInventoryCapacity => Raw?.SelectedRightInventoryCapacity;

		public MemoryStruct.IUIElementText SelectedRightInventoryPathLabel => Raw?.SelectedRightInventoryPathLabel;

		public MemoryStruct.ISprite[] Sprite => Raw?.Sprite;

		public IInventoryTreeViewEntryShip ActiveShipEntry { set; get; }

		public IInventoryCapacityGauge SelectedRightInventoryCapacityMilli { set; get; }

		public ShipCargoSpaceTypeEnum? ActiveShipSelectedCargoSpaceTypeEnum =>
			ActiveShipEntry?.SetCargoSpaceTypeAndTreeEntry?.KeyMap(Bib3.Extension.ToNullable)
			?.FirstOrDefault(CargoSpaceTypeAndTreeEntry => CargoSpaceTypeAndTreeEntry.Value?.IsSelected ?? false).Key;

		public MemoryStruct.ITreeViewEntry ItemHangarEntry { set; get; }

		public int? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		WindowInventory()
		{ }

		public WindowInventory(MemoryStruct.IWindowInventory Raw)
		{
			this.Raw = Raw;

			if (null == Raw)
			{
				return;
			}

			ActiveShipEntry = Raw.TreeEntryActiveShip()?.ParseAsInventoryTreeEntryShip();

			SelectedRightInventoryCapacityMilli = Raw?.SelectedRightInventoryCapacity?.Text?.ParseAsInventoryCapacityGaugeMilli();

			ItemHangarEntry = Raw?.LeftTreeListEntry?.FirstOrDefault(c => c?.Text?.RegexMatchSuccess(@"item\s*hangar", RegexOptions.IgnoreCase) ?? false);
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

		static public IInventoryCapacityGauge ParseAsInventoryCapacityGaugeMilli(this string GaugeString)
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

			return new InventoryCapacityGauge()
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

		static public IEnumerable<KeyValuePair<ShipCargoSpaceTypeEnum, MemoryStruct.ITreeViewEntry>> FromShipExtractSetCargoSpaceTypeAndTreeEntry(
			this MemoryStruct.ITreeViewEntry ShipTreeEntry) =>
			new[] { new KeyValuePair<ShipCargoSpaceTypeEnum, MemoryStruct.ITreeViewEntry>(ShipCargoSpaceTypeEnum.General, ShipTreeEntry) }
			.ConcatNullable(InventoryCargoTypeAndSetLabel?.Select(CargoTypeAndSetLabel =>
			new KeyValuePair<ShipCargoSpaceTypeEnum, MemoryStruct.ITreeViewEntry>(
				CargoTypeAndSetLabel.Key,
				ShipTreeEntry?.EnumerateChildNodeTransitive()
				?.FirstOrDefault(Node => Node?.Text?.FromIventoryLabelParseShipCargoSpaceType() == CargoTypeAndSetLabel.Key))))
			?.Where(TreeEntryForCargoSpaceType => null != TreeEntryForCargoSpaceType.Value);

		static public MemoryStruct.ITreeViewEntry TreeEntryFromCargoSpaceType(
			this MemoryStruct.ITreeViewEntry ShipTreeEntry,
			ShipCargoSpaceTypeEnum CargoSpaceType) =>
			FromShipExtractSetCargoSpaceTypeAndTreeEntry(ShipTreeEntry)
			?.FirstOrDefault(TreeEntryForCargoShipType => TreeEntryForCargoShipType.Key == CargoSpaceType).Value;

		static public ShipCargoSpaceTypeEnum? CargoSpaceTypeFromTreeEntry(
			this MemoryStruct.ITreeViewEntry EntryShip,
			MemoryStruct.ITreeViewEntry EntryCargoSpace) =>
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

		static public MemoryStruct.ITreeViewEntry TreeEntryActiveShip(
			this MemoryStruct.IWindowInventory Inventory) =>
			//	Topmost entry which is a root and has a conforming Label.
			Inventory?.LeftTreeListEntry?.OrderByCenterVerticalDown()
			?.FirstOrDefault(TreeEntry => 0 < TreeEntry?.Text?.ParseTreeEntryLabelShipNameAndType()?.Value?.Length);

		static public IEnumerable<MemoryStruct.ITreeViewEntry> SetTreeEntrySuitingSelectedPath(
			this IEnumerable<MemoryStruct.ITreeViewEntry> SetTreeEntryRoot,
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
				?.Where(TreeEntry => (TreeEntry?.Text?.RemoveXmlTag()?.Trim()).EqualsIgnoreCase(PathListNodeLabelNext))
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
				.Select((LinxTreeEntryPasend) =>
					SetTreeEntrySuitingSelectedPath(
						LinxTreeEntryPasend.Child,
						SelectedPathListNodeLabel.Skip(1)))
				?.Where((Kandidaat) => null != Kandidaat)
				?.ConcatNullable();
		}

		static public IEnumerable<MemoryStruct.ITreeViewEntry> SetLeftTreeEntrySelected(
			this MemoryStruct.IWindowInventory WindowInventory) =>
			WindowInventory?.LeftTreeListEntry?.Select(RootTreeEntry => RootTreeEntry?.EnumerateChildNodeTransitive()).ConcatNullable()
			?.Where(TreeEntry => TreeEntry?.IsSelected ?? false);

		static public ShipCargoSpaceTypeEnum? ActiveShipSelectedCargoSpaceType(
			this MemoryStruct.IWindowInventory WindowInventory) =>
			WindowInventory?.TreeEntryActiveShip()?.FromShipExtractSetCargoSpaceTypeAndTreeEntry()
			?.Where(CargoTypeAndTreeEntry => WindowInventory?.SetLeftTreeEntrySelected()?.Contains(CargoTypeAndTreeEntry.Value) ?? false)
			?.Select(CargoTypeAndTreeEntry => CargoTypeAndTreeEntry.Key)
			?.CastToNullable()
			?.FirstOrDefault();

		static public IInventoryTreeViewEntryShip ParseAsInventoryTreeEntryShip(this MemoryStruct.ITreeViewEntry TreeViewEntry) =>
			null == TreeViewEntry ? null : new WindowInventoryTreeViewShip(TreeViewEntry);

		static public IWindowInventory Parse(this MemoryStruct.IWindowInventory WindowInventory) =>
			null == WindowInventory ? null : new WindowInventory(WindowInventory);
	}
}
