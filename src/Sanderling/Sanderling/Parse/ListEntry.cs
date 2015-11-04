using System;
using BotEngine.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using BotEngine.Interface;
using System.Collections.Generic;

namespace Sanderling.Parse
{
	public interface IListEntry : MemoryStruct.IListEntry
	{
		Int64? DistanceMin { get; }
		Int64? DistanceMax { get; }
		string Name { get; }
		string Type { get; }
		Int64? Quantity { get; }
	}

	public class ListEntry : IListEntry
	{
		MemoryStruct.IListEntry Raw;

		public Int64? DistanceMin { set; get; }

		public Int64? DistanceMax { set; get; }

		public string Name { set; get; }

		public string Type { set; get; }

		public Int64? Quantity { set; get; }

		public IUIElementText[] ButtonText => Raw?.ButtonText;

		public int? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		public int? ContentBoundLeft => Raw?.ContentBoundLeft;

		public IUIElement GroupExpander => Raw?.GroupExpander;

		public long Id => Raw?.Id ?? 0;

		public IUIElementInputText[] InputText => Raw?.InputText;

		public int? InTreeIndex => Raw?.InTreeIndex;

		public bool? IsExpanded => Raw?.IsExpanded;

		public bool? IsGroup => Raw?.IsGroup;

		public bool? IsSelected => Raw?.IsSelected;

		public IUIElementText[] LabelText => Raw?.LabelText;

		public ColorORGB[] ListBackgroundColor => Raw?.ListBackgroundColor;

		public KeyValuePair<IColumnHeader, string>[] ListColumnCellLabel => Raw?.ListColumnCellLabel;


		public RectInt Region => Raw?.Region ?? RectInt.Empty;

		public IUIElement RegionInteraction => Raw?.RegionInteraction;

		public ISprite[] SetSprite => Raw?.SetSprite;

		public ISprite[] Sprite => Raw?.Sprite;

		public ListEntry()
		{
		}

		public ListEntry(MemoryStruct.IListEntry Raw)
		{
			this.Raw = Raw;

			if (null == Raw)
			{
				return;
			}

			var DistanceMinMax = Raw?.ColumnDistanceValue()?.DistanceParseMinMaxKeyValue();

			DistanceMin = DistanceMinMax?.Key;
			DistanceMax = DistanceMinMax?.Value;

			Type = Raw?.ColumnTypeValue();
			Name = Raw?.ColumnNameValue();
		}
	}

	static public class ListEntryExtension
	{
		static Regex NoItemRegex = @"no\s+item".AsRegexCompiledIgnoreCase();

		static public IUIElementText LabelTextLargest(this MemoryStruct.IListEntry ListEntry) =>
			ListEntry?.LabelText?.Largest();

		static public bool IsNoItem(this MemoryStruct.IListEntry ListEntry) =>
			NoItemRegex.MatchSuccess(ListEntry?.LabelTextLargest()?.Text);

		static public string CellValueFromColumnHeader(
			this MemoryStruct.IListEntry ListEntry,
			string HeaderLabel) =>
			ListEntry?.ListColumnCellLabel
			?.FirstOrDefault(Cell => (Cell.Key?.Text).EqualsIgnoreCase(HeaderLabel))
			.Value;

		static public string ColumnTypeValue(this MemoryStruct.IListEntry ListEntry) =>
			CellValueFromColumnHeader(ListEntry, "type");

		static public string ColumnNameValue(this MemoryStruct.IListEntry ListEntry) =>
			CellValueFromColumnHeader(ListEntry, "name");

		static public string ColumnDistanceValue(this MemoryStruct.IListEntry ListEntry) =>
			CellValueFromColumnHeader(ListEntry, "distance");

		static public IListEntry ParseAsListEntry(this MemoryStruct.IListEntry ListEntry) =>
			null == ListEntry ? null : new ListEntry(ListEntry);

		static public IListViewAndControl<OutEntryT> Map<InEntryT, OutEntryT>(
			this IListViewAndControl<InEntryT> ListViewAndControl,
			Func<InEntryT, OutEntryT> EntryMap)
			where InEntryT : MemoryStruct.IListEntry
			where OutEntryT : class, MemoryStruct.IListEntry
		{
			if (null == ListViewAndControl)
			{
				return null;
			}

			return new ListViewAndControl<OutEntryT>(ListViewAndControl)
			{
				ColumnHeader = ListViewAndControl?.ColumnHeader,
				Scroll = ListViewAndControl?.Scroll,
				Entry = ListViewAndControl?.Entry?.Select(EntryMap)?.ToArray(),
			};
		}
	}
}
