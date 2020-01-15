using Bib3;
using BotEngine.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WindowsInput.Native;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	static public class Extension
	{
		static public IMemoryMeasurement Parse(this MemoryStruct.IMemoryMeasurement memoryMeasurement) =>
			null == memoryMeasurement ? null : new Parse.MemoryMeasurement(memoryMeasurement);

		static public IModuleButtonTooltip ParseAsModuleButtonTooltip(this MemoryStruct.IContainer container) =>
			null == container ? null : new ModuleButtonTooltip(container);

		static public INeocom Parse(this MemoryStruct.INeocom neocom) =>
			null == neocom ? null : new Neocom(neocom);

		public const VirtualKeyCode AltKeyCode = VirtualKeyCode.LMENU;
		public const VirtualKeyCode CtrlKeyCode = VirtualKeyCode.CONTROL;

		static VirtualKeyCode? KeyCodeFromUIText(this string keyUIText) =>
			CultureAggregated.SetKeyCodeFromUIText?.CastToNullable()?.FirstOrDefault(uITextAndKeyCode =>
			string.Equals(uITextAndKeyCode?.Key, keyUIText, System.StringComparison.OrdinalIgnoreCase))?.Value;

		static public IEnumerable<VirtualKeyCode> ListKeyCodeFromUIText(this string listKeyUITextAggregated)
		{
			if (null == listKeyUITextAggregated)
				return null;

			var ListKeyText = Regex.Split(listKeyUITextAggregated.Trim(), "-")?.Select(keyText => keyText.Trim())?.ToArray();

			var ListKey = ListKeyText?.Where(keyText => 0 < keyText?.Length)?.Select(KeyCodeFromUIText)?.ToArray();

			if (ListKey?.Any(key => null == key) ?? true)
				return null;

			return ListKey?.WhereNotNullSelectValue();
		}

		static public int? SecondCountFromBracketTimerText(
			this string timerText,
			bool allowLeadingText = false,
			bool allowTrailingText = false)
		{
			const string groupMinuteId = "minute";
			const string groupSecondId = "second";

			var pattern = @"((?<" + groupMinuteId + @">\d+)m\s*|)(?<" + groupSecondId + @">\d{1,2})\s*s";

			if (!allowLeadingText)
				pattern = @"^\s*" + pattern;

			if (!allowTrailingText)
				pattern += @"\s*$";

			var match = timerText?.RegexMatchIfSuccess(pattern, RegexOptions.IgnoreCase);

			if (null == match)
				return null;

			var minuteCount = match.Groups[groupMinuteId]?.Value.TryParseInt() ?? 0;
			var inMinuteSecondCount = match.Groups[groupSecondId]?.Value.TryParseInt();

			return minuteCount * 60 + inMinuteSecondCount;
		}
	}
}
