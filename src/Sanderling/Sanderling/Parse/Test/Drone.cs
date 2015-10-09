using Sanderling.Parse;

namespace Sanderling.Parse.Test
{
	static public class Drone
	{
		static public Bib3.Test.TestCaseMapCompareByRefNezDif<string, DroneLabel>[] TestCaseDroneLabel => new[]
			{
			new Bib3.Test.TestCaseMapCompareByRefNezDif<string, DroneLabel>()
			{
				In = "Hobgoblin I ( <color=0xFFFF0000>Fighting</color> )",
				Out = new DroneLabel()
				{
					Name = "Hobgoblin I",
					Status = "Fighting",
				}
			},
			new Bib3.Test.TestCaseMapCompareByRefNezDif<string, DroneLabel>()
			{
				In = "Hobgoblin I ",
				Out = new DroneLabel()
				{
					Name = "Hobgoblin I",
				}
			},
			};

	}
}
