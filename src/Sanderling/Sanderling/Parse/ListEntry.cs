using BotEngine.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sanderling.Parse
{
	static public class ListEntry
	{
		static Regex NoItemRegex = @"no\s+item".AsRegexCompiledIgnoreCase();

		static public Interface.MemoryStruct.UIElementText LabelTextLargest(this Interface.MemoryStruct.ListEntry ListEntry) =>
			ListEntry?.LabelText?.Largest();

		static public bool IsNoItem(this Interface.MemoryStruct.ListEntry ListEntry) =>
			NoItemRegex.MatchSuccess(ListEntry?.LabelTextLargest()?.Text);

		static public string CellValueFromColumnHeader(
			this Interface.MemoryStruct.ListEntry ListEntry,
			string HeaderLabel) =>
			ListEntry?.ListColumnCellLabel
			?.FirstOrDefault(Cell => (Cell.Key?.Text).EqualsIgnoreCase(HeaderLabel))
			.Value;

		static public string ColumnTypeValue(this Interface.MemoryStruct.ListEntry ListEntry) =>
			CellValueFromColumnHeader(ListEntry, "type");

		static public string ColumnNameValue(this Interface.MemoryStruct.ListEntry ListEntry) =>
			CellValueFromColumnHeader(ListEntry, "name");

		static public string ColumnDistanceValue(this Interface.MemoryStruct.ListEntry ListEntry) =>
			CellValueFromColumnHeader(ListEntry, "distance");

	}
}
