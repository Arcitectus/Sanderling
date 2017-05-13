using Sanderling.Parse;
using NUnit.Framework;
using Bib3.Test;

namespace Sanderling.Test.Exe.Parse
{
	public class Inventory
	{
		[Test]
		public void Parse_Inventory_CapacityGaugeParse() =>
			Sanderling.Parse.Test.Inventory.CapacityGaugeTestCase.AssertMapEquals(
				InventoryExtension.ParseAsInventoryCapacityGaugeMilli,
				new ComparerRefNezDif<IInventoryCapacityGauge>());

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
