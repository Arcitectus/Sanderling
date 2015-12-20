using Bib3;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WindowsInput.Native;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	static public class Extension
	{
		static public IMemoryMeasurement Parse(this MemoryStruct.IMemoryMeasurement MemoryMeasurement) =>
			null == MemoryMeasurement ? null : new Parse.MemoryMeasurement(MemoryMeasurement);

		static public IModuleButtonTooltip ParseAsModuleButtonTooltip(this MemoryStruct.IContainer Container) =>
			null == Container ? null : new ModuleButtonTooltip(Container);

		static public INeocom Parse(this MemoryStruct.INeocom Neocom) =>
			null == Neocom ? null : new Neocom(Neocom);

		const VirtualKeyCode AltKeyCode = VirtualKeyCode.LMENU;
		const VirtualKeyCode CtrlKeyCode = VirtualKeyCode.CONTROL;

		static readonly KeyValuePair<string, VirtualKeyCode>[] SetKeyFCodeFromUIText =
			Enumerable.Range(0, 12).Select(Index => new KeyValuePair<string, VirtualKeyCode>("f" + (Index + 1), VirtualKeyCode.F1 + Index)).ToArray();

		static readonly KeyValuePair<string, VirtualKeyCode>[] SetKeyCodeFromUIText = new[]
		{
			new KeyValuePair<string, VirtualKeyCode>("ctrl", CtrlKeyCode),
			new KeyValuePair<string, VirtualKeyCode>("alt", AltKeyCode),
			new KeyValuePair<string, VirtualKeyCode>("shift", VirtualKeyCode.SHIFT),
			new KeyValuePair<string, VirtualKeyCode>("enter", VirtualKeyCode.RETURN),
			new KeyValuePair<string, VirtualKeyCode>("space", VirtualKeyCode.SPACE),

			//	"de"
			new KeyValuePair<string, VirtualKeyCode>("strg", CtrlKeyCode),
			new KeyValuePair<string, VirtualKeyCode>("umschalt", VirtualKeyCode.SHIFT),
			new KeyValuePair<string, VirtualKeyCode>("eingabe", VirtualKeyCode.RETURN),
			new KeyValuePair<string, VirtualKeyCode>("leer", VirtualKeyCode.SPACE),

			//	TODO: Insert missing mappings here.

		}.Concat(SetKeyFCodeFromUIText).ToArray();

		static VirtualKeyCode? KeyCodeFromUIText(this string KeyUIText) =>
			SetKeyCodeFromUIText?.CastToNullable()?.FirstOrDefault(UITextAndKeyCode =>
			string.Equals(UITextAndKeyCode?.Key, KeyUIText, System.StringComparison.OrdinalIgnoreCase))?.Value;

		static public IEnumerable<VirtualKeyCode> ListKeyCodeFromUIText(this string ListKeyUITextAggregated)
		{
			if (null == ListKeyUITextAggregated)
				return null;

			var ListKeyText = Regex.Split(ListKeyUITextAggregated.Trim(), "-")?.Select(KeyText => KeyText.Trim())?.ToArray();

			var ListKey = ListKeyText?.Where(KeyText => 0 < KeyText?.Length)?.Select(KeyCodeFromUIText)?.ToArray();

			if (ListKey?.Any(Key => null == Key) ?? true)
				return null;

			return ListKey?.WhereNotNullSelectValue();
		}
	}
}
