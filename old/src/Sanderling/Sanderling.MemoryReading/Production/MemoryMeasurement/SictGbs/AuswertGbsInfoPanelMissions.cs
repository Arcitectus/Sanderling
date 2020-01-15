using System;
using System.Collections.Generic;
using System.Linq;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsInfoPanelMissionsMission
	{
		readonly public UINodeInfoInTree AstMission;

		public UINodeInfoInTree AstLabel
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeAstKandidaatMission
		{
			private set;
			get;
		}

		public IUIElementText Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsInfoPanelMissionsMission(UINodeInfoInTree astMission)
		{
			this.AstMission = astMission;
		}

		public void Berecne()
		{
			if (null == AstMission)
				return;

			AstLabel =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstMission, (kandidaat) => string.Equals("EveLabelMedium", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);

			if (null == AstLabel)
				return;

			var AstLabelBescriftung = AstLabel.LabelText();

			Ergeebnis = new UIElementText(AstMission.AsUIElementIfVisible(), AstLabelBescriftung);

			this.Ergeebnis = Ergeebnis;
		}
	}

	public class SictAuswertGbsInfoPanelMissions : SictAuswertGbsInfoPanelGen
	{
		public UINodeInfoInTree[] MengeAstKandidaatMission
		{
			private set;
			get;
		}

		public SictAuswertGbsInfoPanelMissionsMission[] MengeMissionAuswert
		{
			private set;
			get;
		}

		public InfoPanelMissions ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsInfoPanelMissions(UINodeInfoInTree astInfoPanelMissions)
			:
			base(astInfoPanelMissions)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			MengeAstKandidaatMission =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				MainContAst, (kandidaat) => string.Equals("UtilMenu", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2, 1);

			MengeMissionAuswert =
				MengeAstKandidaatMission
				?.Select((astKandidaatMission) =>
					{
						var Auswert = new SictAuswertGbsInfoPanelMissionsMission(astKandidaatMission);

						Auswert.Berecne();

						return Auswert;
					}).ToArray();

			if (null == MengeMissionAuswert)
				return;

			var ListMissionButton =
				MengeMissionAuswert
				.Select((auswert) =>
				{
					var MissionButton = auswert.Ergeebnis;

					var MissionKnopfInGbsFläce = (null == MissionButton) ? RectInt.Empty : MissionButton.Region;

					return new KeyValuePair<IUIElementText, RectInt>(MissionButton, MissionKnopfInGbsFläce);
				})
				.Where((kandidaat) => null != kandidaat.Value)
				.OrderBy((kandidaat) => kandidaat.Value.Center().B)
				.Select((kandidaat) => kandidaat.Key)
				.ToArray();

			ErgeebnisScpez = new InfoPanelMissions(base.Ergeebnis)
			{
				ListMissionButton = ListMissionButton,
			};
		}
	}
}
