using BotEngine.Common;
using System.Linq;
using System.Text.RegularExpressions;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	static public class MenuExtension
	{
		static public MemoryStruct.IMenuEntry EntryFirstMatchingRegexPattern(
			this MemoryStruct.IMenu Menu,
			string RegexPattern,
			RegexOptions RegexOptions = RegexOptions.None) =>
			null == RegexPattern ? null :
			Menu?.Entry?.FirstOrDefault(Entry => Entry?.Text?.RegexMatchSuccess(RegexPattern, RegexOptions) ?? false);
	}
}
