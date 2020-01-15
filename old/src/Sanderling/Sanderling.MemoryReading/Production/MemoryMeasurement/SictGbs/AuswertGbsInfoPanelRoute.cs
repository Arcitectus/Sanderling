using System;
using System.Linq;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsInfoPanelRoute : SictAuswertGbsInfoPanelGen
	{
		public UINodeInfoInTree AstLabelNoDestination
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstMarkersParent
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeAstDestinationMarker
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstCurrentParent
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstEndParent
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstEndParentLabel
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstCurrentParentLabel
		{
			private set;
			get;
		}

		public InfoPanelRoute ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsInfoPanelRoute(UINodeInfoInTree astInfoPanelRoute)
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
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				MainContAst, (kandidaat) => string.Equals("noDestinationLabel", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstMarkersParent =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				MainContAst, (kandidaat) => string.Equals("markersParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstCurrentParent =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				MainContAst, (kandidaat) => string.Equals("currentParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AstCurrentParentLabel =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstCurrentParent, (kandidaat) => string.Equals("EveLabelMedium", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AstEndParent =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				MainContAst, (kandidaat) => string.Equals("endParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AstEndParentLabel =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstEndParent, (kandidaat) => string.Equals("EveLabelMedium", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			MengeAstDestinationMarker =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				AstMarkersParent, (kandidaat) => string.Equals("AutopilotDestinationIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2);

			var MengeMarker =
				MengeAstDestinationMarker
				?.Select((astDestinationMarker) => astDestinationMarker.AsUIElementIfVisible())
				?.ToArray();

			ErgeebnisScpez = new InfoPanelRoute(baseErgeebnis)
			{
				NextLabel = AstCurrentParentLabel.LargestLabelInSubtree().AsUIElementTextIfTextNotEmpty(),
				DestinationLabel = AstEndParentLabel.LargestLabelInSubtree().AsUIElementTextIfTextNotEmpty(),
				RouteElementMarker = MengeMarker?.OrdnungLabel()?.ToArray(),
			};
		}
	}
}
