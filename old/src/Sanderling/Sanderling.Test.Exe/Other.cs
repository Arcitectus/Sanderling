using NUnit.Framework;
using System.Linq;

namespace Sanderling.Test.Exe
{
	public class Other
	{
		class EnumerateReferencedObjectTransitiveTest
		{
			public object Ref;

			public object Ref1;
		}

		[Test]
		public void EnumerateReferencedTransitive_NoCycle()
		{
			var Obj0 = new EnumerateReferencedObjectTransitiveTest();

			var Obj1 = new EnumerateReferencedObjectTransitiveTest() { Ref = Obj0, Ref1 = Obj0 };

			Obj0.Ref = Obj1;
			Obj0.Ref1 = Obj0;

			var Enumerated = Interface.MemoryStruct.Extension.EnumerateReferencedTransitive(Obj0)?.ToArray();

			Assert.AreEqual(2, Enumerated.Length, "Enumerated.Count");

			Assert.IsTrue(Enumerated.Contains(Obj0));
			Assert.IsTrue(Enumerated.Contains(Obj1));
		}
	}
}
