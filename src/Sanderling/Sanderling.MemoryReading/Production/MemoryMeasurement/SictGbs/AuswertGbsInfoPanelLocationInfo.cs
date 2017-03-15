using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsInfoPanelCurrentSystem : SictAuswertGbsInfoPanelGen
	{
		public SictGbsAstInfoSictAuswert AstInfoPanelContainer
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstListSurroundingsBtn
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstHeaderContLabelHeader
		{
			private set;
			get;
		}

		public string TopHeaderLabelText
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMainContLabelNearestLocationInfo
		{
			private set;
			get;
		}

		public InfoPanelSystem ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsInfoPanelCurrentSystem(SictGbsAstInfoSictAuswert astInfoPanelLocationInfo)
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
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				HeaderBtnContAst, (kandidaat) => string.Equals("ListSurroundingsBtn", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 5);

			AstHeaderContLabelHeader =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				HeaderContAst, (kandidaat) => string.Equals("header", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 4);

			AstMainContLabelNearestLocationInfo =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				MainContAst, (kandidaat) =>
					(true == kandidaat.SictbarMitErbe) &&
					string.Equals("nearestLocationInfo", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			if (null != AstHeaderContLabelHeader)
				TopHeaderLabelText = AstHeaderContLabelHeader.SetText;

			IUIElement ButtonListSurroundings = null;

			if (null != AstListSurroundingsBtn)
				ButtonListSurroundings = AstListSurroundingsBtn.AlsUIElementFalsUnglaicNullUndSictbar();

			ErgeebnisScpez = new InfoPanelSystem(BaseErgeebnis)
			{
				ListSurroundingsButton = ButtonListSurroundings,
			};
		}
	}
}
