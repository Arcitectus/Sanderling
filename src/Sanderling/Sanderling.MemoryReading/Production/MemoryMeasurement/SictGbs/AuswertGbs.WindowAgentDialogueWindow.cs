using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowAgentDialogue : SictAuswertGbsWindow
	{
		new static public WindowAgentDialogue BerecneFürWindowAst(
		SictGbsAstInfoSictAuswert windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowAgentDialogue(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisWindowAgentDialogue;
		}

		static public WindowAgentPane PaneAuswert(SictGbsAstInfoSictAuswert paneAst)
		{
			if (!(paneAst?.SictbarMitErbe ?? false))
				return null;

			var EditAst = paneAst?.SuuceFlacMengeAstFrüheste(k => k?.PyObjTypNameEqualsIgnoreCase("Edit") ?? false);

			return
				new WindowAgentPane(paneAst?.AlsUIElementFalsUnglaicNullUndSictbar())
				{
					Html = EditAst?.SrHtmlstr,
				};
		}

		public SictGbsAstInfoSictAuswert LeftPaneAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert RightPaneAst
		{
			private set;
			get;
		}

		/// <summary>
		/// 2014.00.09 Beobactung ("2014.00.09.16 AgentDialogue Request")
		/// "rightPaneBottom" komt in Dialog vor in welcem nur aine "Pane" mit Knöpfe "Request Mission", "Locate Character" und "Close" enthalte sin.
		/// </summary>
		public SictGbsAstInfoSictAuswert RightPaneBottomAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert RightPaneButtonGroupAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstRightPaneTop
		{
			private set;
			get;
		}

		public WindowAgentDialogue ErgeebnisWindowAgentDialogue
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowAgentDialogue(SictGbsAstInfoSictAuswert astFensterAgentDialogueWindow)
			:
			base(astFensterAgentDialogueWindow)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			if (null == base.Ergeebnis)
				return;

			LeftPaneAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMain, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("leftPane", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			RightPaneAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMain, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("rightPane", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			AstRightPaneTop =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				RightPaneAst, (kandidaat) =>
					string.Equals("rightPaneTop", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					3, 1);

			RightPaneBottomAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMain, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("rightPaneBottom", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			ErgeebnisWindowAgentDialogue = new WindowAgentDialogue(
				new WindowAgent(base.Ergeebnis))
			{
				LeftPane = PaneAuswert(LeftPaneAst),
				RightPane = PaneAuswert(RightPaneAst),
			};
		}
	}
}
