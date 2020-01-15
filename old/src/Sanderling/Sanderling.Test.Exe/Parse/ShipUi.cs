using NUnit.Framework;
using System;

namespace Sanderling.Test.Exe.Parse
{
	public class ShipUi
	{
		[Test]
		public void ManeuverTypeFromShipUiIndicationText()
		{
			foreach (var testCase in new[]
			{
				new
				{
					text = "keep	at  range",
					type = ShipManeuverTypeEnum.KeepAtRange,
				},
				new
				{
					text = "orbiting",
					type = ShipManeuverTypeEnum.Orbit,
				},
			})
			{
				try
				{
					Assert.AreEqual(testCase.type, Sanderling.Parse.ShipUiExtension.ManeuverTypeFromShipUiIndicationText(testCase.text));
				}
				catch (Exception exception)
				{
					throw new Exception("failed for case " + testCase.ToString(), exception);
				}
			}
		}
	}
}
