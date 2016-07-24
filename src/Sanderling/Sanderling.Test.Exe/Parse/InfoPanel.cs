using Sanderling.Parse;
using NUnit.Framework;

namespace Sanderling.Test.Exe.Parse
{
	public class InfoPanel
	{
		[Test]
		public void Parse_InfoPanel_System_SecurityLevel()
		{
			Assert.AreEqual(400, @"<url=showinfo:5//1234567 alt='Current Solar System'>Jita</url></b> <color=0xff00ff00L><hint='Security status'>0.4</hint></color><fontsize=12><fontsize=8> </fontsize>&lt;<fontsize=8> </fontsize><url=showinfo:4//1234567>Kimoto</url><fontsize=8> </fontsize>&lt;<fontsize=8> </fontsize><url=showinfo:3//1234567>The Forge</url>".SecurityLevelMilliFromInfoPanelSystemHeaderText());
		}
	}
}
