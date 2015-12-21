using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;

namespace Sanderling.Parse
{
	static public class CultureAggregated
	{
		static VirtualKeyCode CtrlKeyCode => Extension.CtrlKeyCode;
		static VirtualKeyCode AltKeyCode => Extension.AltKeyCode;

		static readonly KeyValuePair<string, VirtualKeyCode>[] SetKeyFCodeFromUIText =
			Enumerable.Range(0, 12).Select(Index => new KeyValuePair<string, VirtualKeyCode>("f" + (Index + 1), VirtualKeyCode.F1 + Index)).ToArray();

		static public readonly KeyValuePair<string, VirtualKeyCode>[] SetKeyCodeFromUIText = new[]
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

		static public IEnumerable<string> KeyCodeFromUITextSetCollidingKey() =>
			SetKeyCodeFromUIText?.GroupBy(KeyCodeFromUIText => KeyCodeFromUIText.Key?.ToLower())?.Where(Group => 1 < Group.Count())?.Select(Group => Group.Key);
	}
}
