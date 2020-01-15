using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsInfoPanelCurrentSystem : SictAuswertGbsInfoPanelGen
	{
		public UINodeInfoInTree AstInfoPanelContainer
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstListSurroundingsBtn
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstHeaderContLabelHeader
		{
			private set;
			get;
		}

		public string TopHeaderLabelText
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstMainContLabelNearestLocationInfo
		{
			private set;
			get;
		}

		public InfoPanelSystem ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsInfoPanelCurrentSystem(UINodeInfoInTree astInfoPanelLocationInfo)
			:
			base(astInfoPanelLocationInfo)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			var BaseErgeebnis = base.Ergeebnis;

			if (null == BaseErgeebnis)
				return;

			AstListSurroundingsBtn =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				HeaderBtnContAst, (kandidaat) => string.Equals("ListSurroundingsBtn", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 5);

			AstHeaderContLabelHeader =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				HeaderContAst, (kandidaat) => string.Equals("header", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 4);

			AstMainContLabelNearestLocationInfo =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				MainContAst, (kandidaat) =>
					(true == kandidaat.VisibleIncludingInheritance) &&
					string.Equals("nearestLocationInfo", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			if (null != AstHeaderContLabelHeader)
				TopHeaderLabelText = AstHeaderContLabelHeader.SetText;

			IUIElement ButtonListSurroundings = null;

			if (null != AstListSurroundingsBtn)
				ButtonListSurroundings = AstListSurroundingsBtn.AsUIElementIfVisible();

			ErgeebnisScpez = new InfoPanelSystem(BaseErgeebnis)
			{
				ListSurroundingsButton = ButtonListSurroundings,
			};
		}
	}
}
