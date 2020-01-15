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

		public IEnumerable<IUIElementText> ButtonText => Raw?.ButtonText;

		public int? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		public int? ContentBoundLeft => Raw?.ContentBoundLeft;

		public IUIElement GroupExpander => Raw?.GroupExpander;

		public long Id => Raw?.Id ?? 0;

		public IEnumerable<IUIElementInputText> InputText => Raw?.InputText;

		public int? InTreeIndex => Raw?.InTreeIndex;

		public bool? IsExpanded => Raw?.IsExpanded;

		public bool? IsGroup => Raw?.IsGroup;

		public bool? IsSelected => Raw?.IsSelected;

		public IEnumerable<IUIElementText> LabelText => Raw?.LabelText;

		public ColorORGB[] ListBackgroundColor => Raw?.ListBackgroundColor;

		public KeyValuePair<IColumnHeader, string>[] ListColumnCellLabel => Raw?.ListColumnCellLabel;


		public RectInt Region => Raw?.Region ?? RectInt.Empty;

		public IUIElement RegionInteraction => Raw?.RegionInteraction;

		public ISprite[] SetSprite => Raw?.SetSprite;

		public IEnumerable<ISprite> Sprite => Raw?.Sprite;

		public ListEntry()
		{
		}

		public ListEntry(MemoryStruct.IListEntry raw)
		{
			this.Raw = raw;

			if (null == raw)
			{
				return;
			}

			var DistanceMinMax = raw?.ColumnDistanceValue()?.DistanceParseMinMaxKeyValue();

			DistanceMin = DistanceMinMax?.Key;
			DistanceMax = DistanceMinMax?.Value;

			Type = raw?.ColumnTypeValue();
			Name = raw?.ColumnNameValue();
		}
	}

	static public class ListEntryExtension
	{
		static Regex NoItemRegex = @"no\s+item".AsRegexCompiledIgnoreCase();

		static public IUIElementText LabelTextLargest(this MemoryStruct.IListEntry listEntry) =>
			listEntry?.LabelText?.Largest();

		static public bool IsNoItem(this MemoryStruct.IListEntry listEntry) =>
			NoItemRegex.MatchSuccess(listEntry?.LabelTextLargest()?.Text);

		static public string CellValueFromColumnHeader(
			this MemoryStruct.IListEntry listEntry,
			string headerLabel) =>
			listEntry?.ListColumnCellLabel
			?.FirstOrDefault(cell => (cell.Key?.Text).EqualsIgnoreCase(headerLabel))
			.Value;

		static public string ColumnTypeValue(this MemoryStruct.IListEntry listEntry) =>
			CellValueFromColumnHeader(listEntry, "type");

		static public string ColumnNameValue(this MemoryStruct.IListEntry listEntry) =>
			CellValueFromColumnHeader(listEntry, "name");

		static public string ColumnDistanceValue(this MemoryStruct.IListEntry listEntry) =>
			CellValueFromColumnHeader(listEntry, "distance");

		static public IListEntry ParseAsListEntry(this MemoryStruct.IListEntry listEntry) =>
			null == listEntry ? null : new ListEntry(listEntry);

		static public IListViewAndControl<OutEntryT> Map<InEntryT, OutEntryT>(
			this IListViewAndControl<InEntryT> listViewAndControl,
			Func<InEntryT, OutEntryT> entryMap)
			where InEntryT : MemoryStruct.IListEntry
			where OutEntryT : class, MemoryStruct.IListEntry
		{
			if (null == listViewAndControl)
			{
				return null;
			}

			return new ListViewAndControl<OutEntryT>(listViewAndControl)
			{
				ColumnHeader = listViewAndControl?.ColumnHeader,
				Scroll = listViewAndControl?.Scroll,
				Entry = listViewAndControl?.Entry?.Select(entryMap)?.ToArray(),
			};
		}
	}
}
