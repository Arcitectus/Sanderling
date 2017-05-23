namespace Sanderling.Parse.Test
{
	static public class Inventory
	{
		static public (string, IInventoryCapacityGauge)[] CapacityGaugeTestCase =>
			new[]{

				("(0,1) 2.153,8 m³", (IInventoryCapacityGauge)new InventoryCapacityGauge{ Used = 2_153_800, Selected = 100, }),

				("0,0/450,0 m³", new InventoryCapacityGauge{ Used = 0, Max = 450_000, }),

				("0.0/8'500.0 m³", new InventoryCapacityGauge{ Used = 0, Max = 8_500_000, }),

				("3 921,5/5 000,0 м³", new InventoryCapacityGauge{ Used = 3_921_500, Max = 5_000_000, }),
			};
	}
}
