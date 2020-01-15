using Sanderling.Parse;
using NUnit.Framework;

namespace Sanderling.Test.Exe.Parse
{
	public class Ore
	{
		[Test]
		public	void Parse_OreTypeEnum()
		{
			Assert.AreEqual(OreTypeEnum.Veldspar, "VeLdspar".AsOreTypeEnum());
			Assert.AreEqual(OreTypeEnum.Dark_Ochre, "DarK OchRe".AsOreTypeEnum());
		}
	}
}
