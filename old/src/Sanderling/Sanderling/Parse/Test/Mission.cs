using System.Collections.Generic;

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

		static public KeyValuePair<string, bool?>[] CaseDialogueDeclineWithoutStandingLossAvailable = new[]
		{
			new KeyValuePair<string, bool?>("Declining a mission from a particular agent more than once every 4 hours may result in a loss of standing with that agent", true),
			new KeyValuePair<string, bool?>("Declining a mission from", false),
		};

		static public KeyValuePair<string, int?>[] CaseDialogueDeclineWithoutStandingLossWaitTime = new[]
		{
			new KeyValuePair<string, int?>("Declining a mission from this agent within the next 4 hours will result in a loss of standing", 4 * 60 * 60),
			new KeyValuePair<string, int?>("Declining a mission from this agent within the next 3 hours and 59 minutes will result in a loss of standing", (3 * 60 + 59) * 60),
			new KeyValuePair<string, int?>("Declining a mission from this agent within the next 4 minutes and 4 seconds will result in a loss of standing", 4 * 60 + 4),
		};
	}

}
