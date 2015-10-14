using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sanderling.Parse
{
	static public class ListEntry
	{
		static Regex NoItemRegex = @"no\s+item".AsRegexCompiledIgnoreCase();

		static public Interface.MemoryStruct.IUIElementText LabelTextLargest(this IListEntry ListEntry) =>
			ListEntry?.LabelText?.Largest();

		static public bool IsNoItem(this IListEntry ListEntry) =>
			NoItemRegex.MatchSuccess(ListEntry?.LabelTextLargest()?.Text);

		static public string CellValueFromColumnHeader(
			this IListEntry ListEntry,
			string HeaderLabel) =>
			ListEntry?.ListColumnCellLabel
			?.FirstOrDefault(Cell => (Cell.Key?.Text).EqualsIgnoreCase(HeaderLabel))
			.Value;

		static public string ColumnTypeValue(this IListEntry ListEntry) =>
			CellValueFromColumnHeader(ListEntry, "type");

		static public string ColumnNameValue(this IListEntry ListEntry) =>
			CellValueFromColumnHeader(ListEntry, "name");

		static public string ColumnDistanceValue(this IListEntry ListEntry) =>
			CellValueFromColumnHeader(ListEntry, "distance");

	}
}
