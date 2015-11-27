using NUnit.Framework;
using Sanderling.Parse;

namespace Sanderling.Test.Exe.Parse
{
	static public class Location
	{
		[Test]
		static public void Parse_Location() =>
			Sanderling.Parse.Test.LocationTestCase.TestCaseLocation.AssertSuccess(label => label.AsLocation());

	}
}
