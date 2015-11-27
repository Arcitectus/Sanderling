namespace Sanderling.Parse.Test
{
	static public class MissionTestCase
	{
		static public Bib3.Test.TestCaseMapCompareByRefNezDif<string, DialogueMissionObjectiveItem>[] CaseDialogueMissionObjectiveItem => new[]
			{
			new Bib3.Test.TestCaseMapCompareByRefNezDif<string, DialogueMissionObjectiveItem>()
			{
				In = " 3 x Crates of Spiced Wine (270,0 m³) ",
				Out = new DialogueMissionObjectiveItem()
				{
					Quantity    = 3,
					Name = "Crates of Spiced Wine",
					VolumeMilli = 270000,
				},
			},
			};


	}

}
