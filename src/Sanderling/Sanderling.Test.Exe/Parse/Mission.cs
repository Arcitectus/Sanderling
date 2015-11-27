using NUnit.Framework;
using Sanderling.Parse;

namespace Sanderling.Test.Exe.Parse
{
	static public class Mission
	{
		[Test]
		static public void Parse_DialogueMission_Item() =>
			Sanderling.Parse.Test.MissionTestCase.CaseDialogueMissionObjectiveItem.AssertSuccess(label => label.ObjectiveItemFromDialogueText());

	}
}
