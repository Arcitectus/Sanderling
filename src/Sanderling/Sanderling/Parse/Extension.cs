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
		static public IMemoryMeasurement Parse(this MemoryStruct.IMemoryMeasurement MemoryMeasurement) =>
			null == MemoryMeasurement ? null : new Parse.MemoryMeasurement(MemoryMeasurement);

		static public IModuleButtonTooltip ParseAsModuleButtonTooltip(this MemoryStruct.IContainer Container) =>
			null == Container ? null : new ModuleButtonTooltip(Container);

		static public INeocom Parse(this MemoryStruct.INeocom Neocom) =>
			null == Neocom ? null : new Neocom(Neocom);

		public const VirtualKeyCode AltKeyCode = VirtualKeyCode.LMENU;
		public const VirtualKeyCode CtrlKeyCode = VirtualKeyCode.CONTROL;

		static VirtualKeyCode? KeyCodeFromUIText(this string KeyUIText) =>
			CultureAggregated.SetKeyCodeFromUIText?.CastToNullable()?.FirstOrDefault(UITextAndKeyCode =>
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
