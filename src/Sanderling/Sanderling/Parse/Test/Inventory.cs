using Bib3.Test;

namespace Sanderling.Parse.Test
{
	static public class Inventory
	{
		static public TestCaseMapCompareByRefNezDif<string, InventoryCapacityGaugeNumeric>[]
			CapacityGaugeTestCase =>
			new TestCaseMapCompareByRefNezDif<string, InventoryCapacityGaugeNumeric>[]{

				new TestCaseMapCompareByRefNezDif<string, InventoryCapacityGaugeNumeric>()
				{
					In = "(0,1) 2.153,8 m³",
					Out = new InventoryCapacityGaugeNumeric()
					{
						Used = 2153800L,
						Selected = 100,
					},
				},

				new TestCaseMapCompareByRefNezDif<string, InventoryCapacityGaugeNumeric>()
				{
					In  = "0,0/450,0 m³",
					Out = new InventoryCapacityGaugeNumeric()
					{
						Used = 0,
						Max = 450L * 1000,
					},
				},

				new TestCaseMapCompareByRefNezDif<string, InventoryCapacityGaugeNumeric>()
				{
					In = "0.0/8'500.0 m³",
					Out = new   InventoryCapacityGaugeNumeric()
					{
						Used = 0,
						Max = 8500L * 1000,
					},
				},
			};

	}
}
