using Bib3;
using System;
using System.Collections.Generic;

namespace Sanderling.Parse.Test
{
	static public class LocationTestCase
	{
		public class TestCaseMapLocation : Bib3.Test.TestCaseMap<string, ILocation, IEqualityComparer<ILocation>>
		{
			public TestCaseMapLocation()
			{
				Comparer = new Func<ILocation, ILocation, bool>(LocationExtension.LocationEquals).EqualityComparerFromFunc();
			}
		}

		static public TestCaseMapLocation[] TestCaseLocation => new[]
			{
			new TestCaseMapLocation()
			{
				In = " Jita ",
				Out = new LocationFromText()
				{
					SystemName = "Jita",
				}
			},
			new TestCaseMapLocation()
			{
				In = "Keikaken V - Moon 1 - Lai Dai Corporation Warehouse ",
				Out = new LocationFromText()
				{
					SystemName = "Keikaken",
					PlanetNumber = 5,
					MoonNumber  = 1,
				}
			},
			};

	}
}
