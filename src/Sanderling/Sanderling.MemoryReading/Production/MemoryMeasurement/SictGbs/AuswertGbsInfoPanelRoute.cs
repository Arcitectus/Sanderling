using System;
using System.Linq;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsInfoPanelRoute : SictAuswertGbsInfoPanelGen
	{
		public SictGbsAstInfoSictAuswert AstLabelNoDestination
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMarkersParent
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] MengeAstDestinationMarker
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstCurrentParent
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstEndParent
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstEndParentLabel
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstCurrentParentLabel
		{
			private set;
			get;
		}

		public InfoPanelRoute ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsInfoPanelRoute(SictGbsAstInfoSictAuswert astInfoPanelRoute)
			:
			base(astInfoPanelRoute)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			var baseErgeebnis = base.Ergeebnis;

			if (null == baseErgeebnis)
				return;

			AstLabelNoDestination =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				MainContAst, (kandidaat) => string.Equals("noDestinationLabel", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstMarkersParent =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				MainContAst, (kandidaat) => string.Equals("markersParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstCurrentParent =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				MainContAst, (kandidaat) => string.Equals("currentParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AstCurrentParentLabel =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstCurrentParent, (kandidaat) => string.Equals("EveLabelMedium", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AstEndParent =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				MainContAst, (kandidaat) => string.Equals("endParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AstEndParentLabel =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstEndParent, (kandidaat) => string.Equals("EveLabelMedium", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			MengeAstDestinationMarker =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAst(
				AstMarkersParent, (kandidaat) => string.Equals("AutopilotDestinationIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2);

			var MengeMarker =
				MengeAstDestinationMarker
				?.Select((astDestinationMarker) => astDestinationMarker.AlsUIElementFalsUnglaicNullUndSictbar())
				?.ToArray();

			ErgeebnisScpez = new InfoPanelRoute(baseErgeebnis)
			{
				NextLabel = AstCurrentParentLabel.GröösteLabel().AsUIElementTextIfTextNotEmpty(),
				DestinationLabel = AstEndParentLabel.GröösteLabel().AsUIElementTextIfTextNotEmpty(),
				RouteElementMarker = MengeMarker?.OrdnungLabel()?.ToArray(),
			};
		}
	}
}
