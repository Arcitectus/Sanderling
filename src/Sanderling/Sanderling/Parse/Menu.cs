using BotEngine.Common;
using System.Linq;
using System.Text.RegularExpressions;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	static public class MenuExtension
	{
		static public MemoryStruct.IMenuEntry EntryFirstMatchingRegexPattern(
			this MemoryStruct.IMenu menu,
			string regexPattern,
			RegexOptions regexOptions = RegexOptions.None) =>
			null == regexPattern ? null :
			menu?.Entry?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccess(regexPattern, regexOptions) ?? false);
	}
}
