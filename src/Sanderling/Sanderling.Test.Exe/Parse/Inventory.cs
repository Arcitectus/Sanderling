using Sanderling.Parse;
using NUnit.Framework;
using System.Collections.Generic;
using Bib3.Test;

namespace Sanderling.Test.Exe.Parse
{
	public class Inventory
	{
		[Test]
		public void Parse_Inventory_CapacityGaugeParse() =>
			Sanderling.Parse.Test.Inventory.CapacityGaugeTestCase.AssertSuccess(gaugeString => gaugeString.ParseAsInventoryCapacityGaugeMilli());

		[Test]
		public void Parse_Inventory_TreeEntry_ShipLabel()
		{
			var ShipName = "Name";
			var ShipType = "Type";
			var TreeEntryLabel = ShipName + "( " + ShipType + " )";

			var Parsed = TreeEntryLabel.ParseTreeEntryLabelShipNameAndType();

			Assert.AreEqual(ShipName, Parsed?.Key);
			Assert.AreEqual(ShipType, Parsed?.Value);
		}
	}
}
