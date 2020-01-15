using Sanderling.Parse;
using NUnit.Framework;

namespace Sanderling.Test.Exe.Parse
{
	public class Distance
	{
		[Test]
		public void Parse_Distance_4km()
		{
			Assert.AreEqual(4000, "4km".DistanceParseMin());
			Assert.AreEqual(4000, "4	  km".DistanceParseMin());
			Assert.AreEqual(4000, "4000m".DistanceParseMin());
		}
	}
}
