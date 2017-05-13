using Bib3.Test;

namespace Sanderling.Parse.Test
{
	static public class Inventory
	{
		static public TestCaseMapCompareByRefNezDif<string, IInventoryCapacityGauge>[]
			CapacityGaugeTestCase =>
			new TestCaseMapCompareByRefNezDif<string, IInventoryCapacityGauge>[]{

				new TestCaseMapCompareByRefNezDif<string, IInventoryCapacityGauge>()
				{
					In = "(0,1) 2.153,8 m³",
					Out = new InventoryCapacityGauge()
					{
						Used = 2153800L,
						Selected = 100,
					},
				},

				new TestCaseMapCompareByRefNezDif<string, IInventoryCapacityGauge>()
				{
					In  = "0,0/450,0 m³",
					Out = new InventoryCapacityGauge()
					{
						Used = 0,
						Max = 450L * 1000,
					},
				},

				new TestCaseMapCompareByRefNezDif<string, IInventoryCapacityGauge>()
				{
					In = "0.0/8'500.0 m³",
					Out = new   InventoryCapacityGauge()
					{
						Used = 0,
						Max = 8500L * 1000,
					},
				},

				new TestCaseMapCompareByRefNezDif<string, IInventoryCapacityGauge>()
				{
					In = "3 921,5/5 000,0 м³",
					Out = new   InventoryCapacityGauge()
					{
						Used = 3921500L,
						Max = 5000L * 1000
					},
				},
			};

	}
}
