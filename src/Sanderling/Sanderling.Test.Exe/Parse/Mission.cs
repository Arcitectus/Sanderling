using Bib3;
using NUnit.Framework;
using Sanderling.Parse;

namespace Sanderling.Test.Exe.Parse
{
	static public class Mission
	{
		[Test]
		static public void Parse_DialogueMission_Item() =>
			Sanderling.Parse.Test.MissionTestCase.CaseDialogueMissionObjectiveItem.AssertSuccess(label => label.ObjectiveItemFromDialogueText());

		[Test]
		static public void Parse_DialogueMission_DeclineWithoutStandingLossAvailable() =>
			Sanderling.Parse.Test.MissionTestCase.CaseDialogueDeclineWithoutStandingLossAvailable.AssertObjectEquals(html => html.DeclineWithoutStandingLossAvailableFromDialogueHtml());

		[Test]
		static public void Parse_DialogueMission_DeclineWithoutStandingLossWaitTime() =>
			Sanderling.Parse.Test.MissionTestCase.CaseDialogueDeclineWithoutStandingLossWaitTime.AssertObjectEquals(html => html.DeclineWithoutStandingLossWaitTimeFromDialogueHtml());
	}
}
