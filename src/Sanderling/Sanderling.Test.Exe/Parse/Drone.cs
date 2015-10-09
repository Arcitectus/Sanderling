using NUnit.Framework;
using Sanderling.Parse;

namespace Sanderling.Test.Exe.Parse
{
	static public class Drone
	{
		[Test]
		static public void Parse_DroneLabel() =>
			Sanderling.Parse.Test.Drone.TestCaseDroneLabel.AssertSuccess(label => label.AsDroneLabel());

	}
}
