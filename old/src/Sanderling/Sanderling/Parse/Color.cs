using BotEngine.Interface;

namespace Sanderling.Parse
{
	static public class ColorExtension
	{
		static public bool IsRed(this ColorORGB color) =>
			null != color &&
			color.BMilli < color.RMilli / 3 &&
			color.GMilli < color.RMilli / 3 &&
			300 < color.RMilli;
	}
}
